﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;

using umbraco.BusinessLogic; 

using System.IO ;
using umbraco; 

using Umbraco.Core.IO ;
using Umbraco.Core.Logging;

using jumps.umbraco.usync.helpers;
using jumps.umbraco.usync.Models;
using System.Xml.Linq;

namespace jumps.umbraco.usync
{
    /// <summary>
    /// Sycronizes stylesheets to the uSync folder. 
    /// 
    /// stylesheets are mainly arealy on the disk, the database
    /// contains an ID for each one - it's only ever used in
    /// rich text data type (i think).
    /// 
    /// SyncStylesheet class uses the packaging API to read and
    /// write the styles sheets to disk. 
    /// 
    /// probibly the simplest sync - no structure, and the
    /// packaging api.
    /// </summary>
    public class SyncStylesheet : SyncItemBase<StyleSheet>
    {
        public SyncStylesheet() :
            base(uSyncSettings.Folder) { }

        public SyncStylesheet(string folder) :
            base(folder) { }

        public SyncStylesheet(string folder, string set) :
            base(folder, set) { }

        public override void ExportAll(string folder)
        {
            foreach (StyleSheet item in StyleSheet.GetAll())
            {
                ExportToDisk(item, folder);
            }
        }

        public override void ExportToDisk(StyleSheet item, string folder = null)
        {
            if (item == null)
                throw new ArgumentNullException("item");

            if (string.IsNullOrEmpty(folder))
                folder = _savePath;

            try
            {
                var node = item.SyncExport();
                XmlDoc.SaveNode(folder, item.Text, node, Constants.ObjectTypes.Stylesheet);
            }
            catch (Exception ex)
            {
                LogHelper.Info<SyncStylesheet>("uSync: Error Reading Stylesheet {0} - {1}", () => item.Text, () => ex.ToString());
            }
        }

        public override void ImportAll(string folder)
        {
            string root = IOHelper.MapPath(string.Format("{0}\\{1}", folder, Constants.ObjectTypes.Stylesheet));
            base.ImportFolder(root);
        }

        public override void Import(string filePath)
        {
            if ( !File.Exists(filePath))            
                throw new ArgumentNullException("filepPath");

            XElement node = XElement.Load(filePath);

            if ( node.Name.LocalName != "Stylesheet")
                throw new ArgumentException("Not a stylesheet file", filePath);

            if (tracker.StylesheetChanged(node))
            {
                var backup = Backup(node);

                ChangeItem change = uStylesheet.SyncImport(node);

                if (change.changeType == ChangeType.Mismatch)
                    Restore(backup);

                AddChange(change);
            }
            else
                AddNoChange(ItemType.Stylesheet, filePath);
        }

        protected override string Backup(XElement node)
        {
            var name = node.Element("Name").Value;
            var stylesheet = StyleSheet.GetByName(name);

            if (stylesheet != null)
            {
                ExportToDisk(stylesheet, _backupPath);
                return XmlDoc.GetSavePath(_backupPath, name, Constants.ObjectTypes.Stylesheet);
            }

            return "" ;
        }

        protected override void Restore(string backup)
        {
            XElement backupNode = XmlDoc.GetBackupNode(backup);

            if (backupNode != null)
                uStylesheet.SyncImport(backupNode, false);
        }

        static string _eventFolder = "";

        public static void AttachEvents(string folder)
        {
            _eventFolder = folder;
            StyleSheet.AfterSave += StyleSheet_AfterSave;
            StyleSheet.BeforeDelete += StyleSheet_BeforeDelete;
           
        }


        static void StyleSheet_BeforeDelete(StyleSheet sender, DeleteEventArgs e)
        {
            XmlDoc.ArchiveFile(XmlDoc.GetSavePath(_eventFolder, sender.Text, Constants.ObjectTypes.Stylesheet), true);
            e.Cancel = false;
        }
        

        static void StyleSheet_AfterSave(StyleSheet sender, SaveEventArgs e)
        {
            var styleSync = new SyncStylesheet();
            styleSync.ExportToDisk(sender,_eventFolder); 
        }
    }
}