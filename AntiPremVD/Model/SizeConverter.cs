namespace AntiPremVD.Model
{
    public static class SizeConverter
    {
        public static string FormatFileSize(long fileSizeInBytes)
        {
            const long oneKB = 1024;
            const long oneMB = oneKB * 1024;
            const long oneGB = oneMB * 1024;

            if (fileSizeInBytes >= oneGB)
            {
                return $"{(fileSizeInBytes / (double)oneGB):0.##} GB"; 
            }
            else if (fileSizeInBytes >= oneMB)
            {
                return $"{(fileSizeInBytes / (double)oneMB):0.##} MB"; 
            }
            else if (fileSizeInBytes >= oneKB)
            {
                return $"{(fileSizeInBytes / (double)oneKB):0.##} KB"; 
            }
            else
            {
                return $"{fileSizeInBytes} B";
            }
        }
    }
}
