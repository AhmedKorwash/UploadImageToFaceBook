using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace WebScraping_Article_Codes
{
    class UploadImage
    {
        public string UploadPhoto(string message, string filename, Byte[] bytes, string Token, string locat)
        {
            // Create Boundary
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");

            // Create Path
            //locat >> me , or group id , or frind id , you can using Alboum id
            string Path = string.Format(@"https://graph.facebook.com/{0}/photos", locat);
            // Create HttpWebRequest
            HttpWebRequest uploadRequest;
            uploadRequest = (HttpWebRequest)HttpWebRequest.Create(Path);
            uploadRequest.ServicePoint.Expect100Continue = false;
            uploadRequest.Method = "POST";
            uploadRequest.UserAgent = "Mozilla/4.0 (compatible; Windows NT)";
            uploadRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            uploadRequest.KeepAlive = false;

            // New String Builder
            StringBuilder sb = new StringBuilder();

            // Add Form Data
            string formdataTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"\r\n\r\n{2}\r\n";

            // Access Token
            sb.AppendFormat(formdataTemplate, boundary, "access_token", Token);

            // Message
            sb.AppendFormat(formdataTemplate, boundary, "message", message);

            // Header
            string headerTemplate = "--{0}\r\nContent-Disposition: form-data; name=\"{1}\"; filename=\"{2}\"\r\nContent-Type: {3}\r\n\r\n";
            sb.AppendFormat(headerTemplate, boundary, "source", filename, @"application/octet-stream");

            // File
            string formString = sb.ToString();
            byte[] formBytes = Encoding.UTF8.GetBytes(formString);
            byte[] trailingBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");
            byte[] image;
            if (bytes == null)
            {
                image = File.ReadAllBytes(filename);
            }
            else
            {
                image = bytes;
            }

            // Memory Stream
            MemoryStream imageMemoryStream = new MemoryStream();
            imageMemoryStream.Write(image, 0, image.Length);

            // Set Content Length
            long imageLength = imageMemoryStream.Length;
            long contentLength = formBytes.Length + imageLength + trailingBytes.Length;
            uploadRequest.ContentLength = contentLength;

            // Get Request Stream
            uploadRequest.AllowWriteStreamBuffering = false;
            Stream strm_out = uploadRequest.GetRequestStream();

            // Write to Stream
            strm_out.Write(formBytes, 0, formBytes.Length);
            byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)imageLength))];
            int bytesRead = 0;
            int bytesTotal = 0;
            imageMemoryStream.Seek(0, SeekOrigin.Begin);
            while ((bytesRead = imageMemoryStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                strm_out.Write(buffer, 0, bytesRead); bytesTotal += bytesRead;
            }
            strm_out.Write(trailingBytes, 0, trailingBytes.Length);

            // Close Stream
            strm_out.Close();

            // Get Web Response
            HttpWebResponse response = uploadRequest.GetResponse() as HttpWebResponse;

            // Create Stream Reader
            StreamReader reader = new StreamReader(response.GetResponseStream());

            // Return
            return reader.ReadToEnd();
        }
    }
}
