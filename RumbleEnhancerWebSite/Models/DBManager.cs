using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml;

namespace RumbleEnhancerWebSite.Models
{
    public static class DBManager
    {
        static SqlConnection con;
        static SqlCommand cmd;

        public static void DBSetup()
        {
            con = new SqlConnection(GetConString());
            cmd = new SqlCommand();
            cmd.Connection = con;
            cmd.CommandType = CommandType.Text;
        }

        public static string GetConString()
        {
            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var config = builder.Build();
            string constring = config.GetConnectionString("DefaultConnectionString");
            return constring;
        }

        public static Profile CreateProfile(Profile profile)
        {
            Profile successProfile = new Profile();
            var hashedPW = BCrypt.Net.BCrypt.HashPassword(profile.ProfilePassword);
            profile.ProfilePassword = hashedPW;
            if (GetVerificationStatus(profile))
                return successProfile;

            //TODO: Make a check for if email or rumble name is already verified
            cmd.CommandText = "INSERT INTO [Profiles](ProfileName, ProfilePassword, ProfileEmail) values ('" + profile.ProfileName + "','" + hashedPW + "', '" + profile.ProfileEmail + "')";

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                successProfile = new Profile()
                {
                    ProfileEmail = profile.ProfileEmail,
                    ProfileName = profile.ProfileName,
                    ProfilePassword = hashedPW
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return successProfile;
        }

        public static bool GetVerificationStatus(Profile profile)
        {
            cmd.CommandText = "SELECT * FROM [Profiles] WHERE ProfileName=@ProfileName";
            cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
            bool isNameVerified = false;
            bool isEmailVerified = false;
            con.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                isNameVerified = reader.GetBoolean(reader.GetOrdinal("ProfileVerified"));
                if (isNameVerified)
                {
                    con.Close();
                    return true;
                }

            }
            con.Close();

            cmd.Parameters.Clear();

            con.Open();
            cmd.CommandText = "SELECT * FROM [Profiles] WHERE ProfileEmail=@ProfileEmail";
            cmd.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);

            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                isEmailVerified = reader.GetBoolean(reader.GetOrdinal("ProfileVerified"));
                if (isEmailVerified)
                {
                    con.Close();
                    return true;
                }
            }
            con.Close();

            return (isNameVerified || isEmailVerified);
        }

        public static Profile LogIn(Profile profile)
        {
            bool passwordCorrect = false;
            cmd.CommandText = "SELECT * FROM [Profiles] WHERE ProfileEmail=@ProfileEmail";
            cmd.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);

            var loggedInProfile = new Profile();
            con.Open();
            try
            {
                var reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    passwordCorrect = BCrypt.Net.BCrypt.Verify(profile.ProfilePassword, reader.GetString(reader.GetOrdinal("ProfilePassword")));
                    profile.ProfileName = reader.GetString(reader.GetOrdinal("ProfileName"));
                    profile.ProfilePassword = reader.GetString(reader.GetOrdinal("ProfilePassword"));
                    profile.ProfileEmail = reader.GetString(reader.GetOrdinal("ProfileEmail"));
                    profile.VerificationString = reader.GetGuid(reader.GetOrdinal("ProfileId")).ToString();
                    profile.VerificationStatus = reader.GetBoolean(reader.GetOrdinal("ProfileVerified"));
                }
            }
            catch(Exception e) 
            {
                Console.WriteLine(e.Message);
            };

            con.Close();
            if (passwordCorrect)
            {
                loggedInProfile = GetProfile(profile);
            }
            /*if (passwordCorrect)
            {
                cmd.CommandText = "SELECT * FROM [VerifiedProfiles] WHERE VerifiedName=@ProfileName";
                cmd.Parameters.AddWithValue("@VerifiedName", profile.ProfileName);
                con.Open();
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    verify[1] = reader.GetString(reader.GetOrdinal("VerifiedName")) == profile.ProfileName;
                }
                con.Close();
            }*/
            return loggedInProfile;
        }

        public static bool VerifyProfile(Profile profile)
        {
            //Check the link
            //TODO: IMPLEMENT DESCRIPTION CONFIRMER
            string[] videoData = GetVideoData(profile.RumbleURL);
            if (!(videoData[0] == profile.ProfileName && videoData[1].Contains(profile.VerificationString)))
            {
                return false;
            }

            //if (!(profile.RumbleURL == ))
                //return false;

            //Verify in db
            try
            {
                con.Open();
                cmd.CommandText = "UPDATE [Profiles] SET ProfileVerified = '1' WHERE ProfileId=@ProfileId";
                cmd.Parameters.AddWithValue("@ProfileId", profile.VerificationString);
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Verification failed" + e.Message);
            }

            return true;
            /*
            cmd.CommandText = "SELECT * FROM [Profiles] WHERE ProfileName=@ProfileName";
            cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName);

            con.Open();
            var reader = cmd.ExecuteReader();
            var prof = new Profile();
            string id = "temp";
            while (reader.NextResult())
            {
                id = reader.GetGuid(reader.GetOrdinal("ProfileId")).ToString();
                if (id.Equals(profile.VerificationString))
                {

                }
                prof.ProfileName = profile.ProfileName;
                prof.ProfileEmail = reader.GetString(reader.GetOrdinal("ProfileEmail"));
                prof.ProfilePassword = reader.GetString(reader.GetOrdinal("ProfilePassword"));

            }
            con.Close();

            cmd.CommandText = "INSERT INTO [VerifiedProfiles](VerifiedName, VerifiedPassword, VerifiedEmail, VerifiedID) values ('" + prof.ProfileName + "','" + prof.ProfilePassword + "', '" + prof.ProfileEmail + "', '" + id + "')";

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

            }*/
        }

        public static List<Profile> GetVerificationStrings(Profile profile)
        {
            cmd.CommandText = "SELECT * FROM [Profiles] WHERE ProfileName=@ProfileName";
            cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
            con.Open();
            var reader = cmd.ExecuteReader();
            List<Profile> verificationStrings = new List<Profile>();
            while (reader.NextResult())
            {
                Profile tempProfile = new Profile()
                {
                    ProfileEmail = reader.GetString(reader.GetOrdinal("ProfileEmail")),
                    ProfileName = reader.GetString(reader.GetOrdinal("ProfileName")),
                    ProfilePassword = reader.GetString(reader.GetOrdinal("ProfilePassword")),
                    VerificationString = reader.GetGuid(reader.GetOrdinal("ProfileId")).ToString(),
                    VerificationStatus = reader.GetBoolean(reader.GetOrdinal("ProfileVerified"))
                };
                verificationStrings.Add(tempProfile);
            }
            con.Close();
            return verificationStrings;
        }

        public static Profile GetProfile(Profile profile)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT * FROM [Profiles] WHERE ProfileName=@ProfileName AND ProfileEmail=@ProfileEmail AND ProfilePassword=@ProfilePassword";
            cmd.Parameters.AddWithValue("@ProfileName", profile.ProfileName);
            cmd.Parameters.AddWithValue("@ProfilePassword", profile.ProfilePassword);
            cmd.Parameters.AddWithValue("@ProfileEmail", profile.ProfileEmail);
            con.Open();
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                profile.VerificationString = reader.GetGuid(reader.GetOrdinal("ProfileId")).ToString();
                profile.VerificationStatus = reader.GetBoolean(reader.GetOrdinal("ProfileVerified"));
            }
            con.Close();
            return profile;
        }

        public static string GetProfileId(string profileName)
        {
            string profileId = "";
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT * FROM [Profiles] WHERE ProfileName='" + profileName + "' AND ProfileVerified='1'";
            con.Open();
            var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                profileId = reader.GetGuid(reader.GetOrdinal("ProfileId")).ToString();
            }
            con.Close();
            return profileId;
        }

        public static void SendEmail(Profile profile)
        {
            string to = profile.ProfileEmail; //To address    
            string from = "noreplyrumbleenhancer@gmail.com"; //From address    
            MailMessage message = new MailMessage(from, to);

            string mailbody = "Verify your RumbleEnhancer account by typing the following identifier into the description of a video of yours. <br/> Identifier: " + profile.VerificationString;
            message.Subject = "Verify your RumbleEnhancer";
            message.Body = mailbody;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = true;
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587); //Gmail smtp    
            System.Net.NetworkCredential basicCredential1 = new
            System.Net.NetworkCredential("noreplyrumbleenhancer@gmail.com", "zggtniiqvsnxwxkn");
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.Credentials = basicCredential1;
            try
            {
                client.Send(message);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string[] GetVideoData(string url)
        {
            string HTML;
            using (var wc = new WebClient())
            {
                HTML = wc.DownloadString(url);
            }
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(HTML);

            string[] data = { "", "" };
            HtmlNode accountName = doc.DocumentNode.SelectSingleNode("//div[@class='media-heading-name']");
            HtmlNode description = doc.DocumentNode.SelectSingleNode("//p[@class='media-description']");
            if (description != null)
            {
                 data[0] = accountName.InnerHtml.ToString();
                 data[1] = description.InnerHtml.ToString();
            }
            return data;
        }

        public static bool UploadEmote(EmoteModel emote)
        {
            bool success = false;
            cmd.CommandText = "INSERT INTO Emotes VALUES (@ProfileId, @EmoteName, @ImageData)";
            cmd.Parameters.AddWithValue("@ProfileId", emote.ProfileId);
            cmd.Parameters.AddWithValue("@EmoteName", emote.EmoteName);
            cmd.Parameters.AddWithValue("@ImageData", emote.ImageData);

            try
            {
                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
                success = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                success = false;
            }

            return success;
        }

        public static List<EmoteModel> GetProfileEmotes(string profileId)
        {
            List<EmoteModel> emotes = new List<EmoteModel>();
            cmd.CommandText = "SELECT * FROM [Emotes] WHERE ProfileId='" + profileId + "'";
            try
            {
                con.Open();
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var emote = new EmoteModel()
                    {
                        ProfileId = reader.GetGuid(reader.GetOrdinal("ProfileId")).ToString(),
                        EmoteName = reader.GetString(reader.GetOrdinal("EmoteName")),
                        ImageData = (byte[])reader[reader.GetOrdinal("ImageData")]
                    };
                    emotes.Add(emote);
                }
                con.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return emotes;
        }

    }
}
