﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO ; 
using System.Xml ;
using Umbraco.Core.IO ; 

namespace jumps.umbraco.usync.helpers
{
    /// <summary>
    /// helper class, does the bits making sure our 
    /// xml is consistantly created, and put in some
    /// form of logical place. 
    /// </summary>
    public class XmlDoc
    {
        public static XmlDocument CreateDoc()
        {
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "no");
            doc.AppendChild(dec);

            return doc;
        }

        public static void SaveXmlDoc(string type, string name, XmlDocument doc)
        {
            string savePath = string.Format("{0}/{1}.config", type, ScrubFile(name)) ;
            SaveXmlDoc(savePath, doc) ; 
        }

        public static void SaveXmlDoc(string path, XmlDocument doc)
        {
            string savePath = string.Format("{0}/{1}", IOHelper.MapPath(uSyncIO.RootFolder), path);

            if ( !Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }
            else {
                if ( File.Exists(savePath) ) 
                {
                    // TODO: Archive here..? 
                    File.Delete(savePath);
                }
            }

            doc.Save(savePath) ; 
        }

        public static void ArchiveFile(string type, string name)
        {
            string liveRoot = IOHelper.MapPath(uSyncIO.RootFolder);
            string archiveRoot = IOHelper.MapPath(uSyncIO.ArchiveFolder);

            string currentFile = string.Format(@"{0}\{1}\{2}.config",
                liveRoot, type, ScrubFile(name));


            string archiveFile = string.Format(@"{0}\{1}\{2}_{3}.config",
                archiveRoot, type, ScrubFile(name), DateTime.Now.ToString("ddMMyy_hhmmss"));


            // we need to confirm the archive directory exists 
            if (!Directory.Exists(Path.GetDirectoryName(archiveFile)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(archiveFile));
            }

            if (File.Exists(currentFile))
            {
                // it shouldn't happen as we are going for a unique name
                // but it might be called twice v'quickly

                if (File.Exists(archiveFile))
                {
                    File.Delete(archiveFile);
                }

                // 
                File.Copy(currentFile, archiveFile);
                File.Delete(currentFile); 
            }

        }

       
        /// <summary>
        /// we need to clean the name up to make it a valid file name..
        /// </summary>
        /// <param name="filename"></param>
        public static string ScrubFile(string filename)
        {
            // TODO: a better scrub

            StringBuilder sb = new StringBuilder(filename);
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char item in invalid)
            {
                sb.Replace(item.ToString(), "");
            }

            return sb.ToString() ;
        }

        public static string GetNodeValue(XmlNode val)
        {
            string value = val.Value;

            if (String.IsNullOrEmpty(value))
                return "";
            else
                return value;
        }
        
    }
}
