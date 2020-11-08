using System;
using System.Collections.Generic;
using System.Text;

namespace Ios.Backup.Decrypter.Console.Models
{
    public class IosPathsFiles
    {
        // Standard iOS file locations:
        public static string CAMERA_ROLL = "Media/DCIM/%APPLE/IMG%.%";
        public static string SMS_ATTACHMENTS = "Library/SMS/Attachments/%.%";

        // WhatsApp paths, which contain "."s and so must search for ".jpg" and ".mp4" individually:
        public static string WHATSAPP_ATTACHED_IMAGES = "Message/Media/%.jpg";
        public static string WHATSAPP_ATTACHED_VIDEOS = "Message/Media/%.mp4";

        
        

        
        
    }
}
