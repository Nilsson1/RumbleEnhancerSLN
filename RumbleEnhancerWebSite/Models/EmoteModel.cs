namespace RumbleEnhancerWebSite.Models
{
    public class EmoteModel
    {
        public string ProfileId { get; set; }
        public string EmoteName { get; set; }
        public byte[] ImageData { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
