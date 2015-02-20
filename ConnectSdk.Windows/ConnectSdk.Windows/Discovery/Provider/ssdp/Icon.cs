namespace ConnectSdk.Windows.Discovery.Provider.ssdp
{
    public class Icon 
    {
        public static string Tag = "icon";
        public static string TagMimeType = "mimetype";
        public static string TagWidth = "width";
        public static string TagHeight = "height";
        public static string TagDepth = "depth";
        public static string TagUrl = "url";
	
        /// <summary>
        /// Required. Icon's MIME type.
        /// </summary>
        public string MimeType;

        /// <summary>
        /// Required. Horizontal dimension of icon in pixels.
        /// </summary>
        public string Width;

        /// <summary>
        /// Required. Vertical dimension of icon in pixels.
        /// </summary>
        public string Height;

        /// <summary>
        /// Required. Number of color bits per pixel. 
        /// </summary>
        public string Depth;

        /// <summary>
        /// Required. Pointer to icon image.
        /// </summary>
        public string Url;
    }
}