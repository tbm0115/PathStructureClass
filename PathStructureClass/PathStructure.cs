using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Text;
using System.Xml;
using HTML;
using HTML.HTMLWriter;
using HTML.HTMLWriter.HTMLTable;

namespace PathStructureClass
{
    /// <summary>
    /// C# rewrite scaffold of the original VB.NET PathStructure implementation.
    /// </summary>
    public class PathStructure
    {
        private bool _ERPCheck;
        private XmlDocument _myXML;
        private bool _DeleteThumbs;
        private bool _HandleExtensions;
        private bool _generateIcons;
        private bool _setPermissions;
        private string _ERPConnection;
        public List<string> defaultPaths = new List<string>();
        private List<StructureStyle> _structs;
        public DatabaseConnection ERPConnection;
        private string _xmlPath;

        public bool IsNull()
        {
            return ReferenceEquals(this, null);
        }

        public string SettingsPath => _xmlPath;

        public bool CheckERPSystem
        {
            get => _ERPCheck;
            set => _ERPCheck = value;
        }

        public XmlDocument Settings
        {
            get => _myXML;
            set => _myXML = value;
        }

        public bool AllowDeletionOfThumbsDb
        {
            get => _DeleteThumbs;
            set => _DeleteThumbs = value;
        }

        public string ERPSystemConnectionString
        {
            get => _ERPConnection;
            set => _ERPConnection = value;
        }

        /// <summary>
        /// Sets whether or not a Path object will handle its extension during the IsNamedStructure() routine.
        /// </summary>
        public bool HandleExtensions
        {
            get => _HandleExtensions;
            set => _HandleExtensions = value;
        }

        /// <summary>
        /// Sets whether or not the PathStructure will generate icon files while building the audit report.
        /// </summary>
        public bool GenerateIcons
        {
            get => _generateIcons;
            set => _generateIcons = value;
        }

        /// <summary>
        /// Sets whether or not the PathStructure will set permissions while building the audit report.
        /// </summary>
        public bool SetPermissions
        {
            get => _setPermissions;
            set => _setPermissions = value;
        }

        public List<StructureStyle> Structures => _structs;

        public PathStructure()
        {
        }

        public PathStructure(string xmlPath)
        {
            _xmlPath = xmlPath;
        }

        public PathStructure(XmlDocument settings)
        {
            _myXML = settings;
        }

        public PathStructure(XmlDocument settings, bool checkERPSystem)
        {
            _myXML = settings;
            _ERPCheck = checkERPSystem;
        }

        public virtual void Load(string xmlPath)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public virtual void Load(XmlDocument settings)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public virtual string ReplaceVariables(string path, VariableArray vars = null)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public virtual string XPathToPath(string xpath)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public virtual string XPathToPath(XmlElement xmlElement)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public virtual string PathToXPath(string path)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public virtual Path FindPath(string name)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public virtual Path FindPath(string name, string ext)
        {
            throw new NotImplementedException("Full conversion required.");
        }

        public class StructureStyle
        {
            public string Name { get; set; }
            public string DefaultPath { get; set; }
            public XmlElement Structure { get; set; }
            public XmlElement Defaults { get; set; }
            public XmlElement Permissions { get; set; }
            public XmlElement Extensions { get; set; }
            public XmlElement Audit { get; set; }
            public XmlElement Users { get; set; }
            public List<PathStyle> PathStyles { get; set; }
        }

        public class PathStyle
        {
            public string Name { get; set; }
            public PathStyleType Type { get; set; }
            public string XPath { get; set; }
            public string Path { get; set; }
            public string Regex { get; set; }
            public XmlElement Node { get; set; }
        }

        public enum PathStyleType
        {
            Path,
            File
        }
    }

    public class Path : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public class VariableArray
    {
    }

    public class Variable
    {
    }

    public class StructureCandidateArray
    {
    }

    public class StructureCandidate
    {
    }

    public class Extensions
    {
    }

    public class DatabaseConnection
    {
    }

    public class Users
    {
    }
}
