using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Xml;

namespace PathStructureClass
{
    public static class PathStructure_Helpers
    {
        /// <summary>
        /// Gets the index of the Nth occurrence of a character.
        /// </summary>
        [Extension]
        public static int GetNthIndexOf(this string input, char ch, int index)
        {
            var count = 0;
            for (var i = 0; i < input.Length; i += 1)
            {
                if (input[i] == ch)
                {
                    count += 1;
                    if (count == index)
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        public enum StringContainsType
        {
            ContainsOr,
            ContainsAnd
        }

        [Extension]
        public static bool Contains(this string input, string[] condition, StringContainsType containsType = StringContainsType.ContainsOr)
        {
            var cond = false;
            for (var i = 0; i < condition.Length; i += 1)
            {
                if (input.IndexOf(condition[i], StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    cond = true;
                    if (containsType == StringContainsType.ContainsOr)
                    {
                        break;
                    }
                }
                else
                {
                    if (containsType == StringContainsType.ContainsAnd)
                    {
                        cond = false;
                        break;
                    }
                }
            }

            return cond;
        }

        /// <summary>
        /// Gets the index of the base element within its parent.
        /// </summary>
        [Extension]
        private static int FindElementIndex(this XmlElement element)
        {
            var parentNode = element.ParentNode;
            if (parentNode.NodeType == XmlNodeType.Document)
            {
                return 1;
            }

            var parent = (XmlElement)parentNode;
            var index = 1;
            foreach (XmlNode candidate in parent.ChildNodes)
            {
                if (candidate.NodeType == XmlNodeType.Element && candidate.Name == element.Name)
                {
                    if (((XmlElement)candidate).Equals(element))
                    {
                        return index;
                    }

                    index += 1;
                }
            }

            throw new ArgumentException("Couldn't find element within parent");
        }

        /// <summary>
        /// Gets the XPath string for the base XmlNode.
        /// </summary>
        [Extension]
        public static string FindXPath(this XmlNode node)
        {
            var builder = new StringBuilder();
            while (node != null)
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Attribute:
                        builder.Insert(0, "/@" + node.Name);
                        node = ((XmlAttribute)node).OwnerElement;
                        continue;
                    case XmlNodeType.Element:
                        var index = ((XmlElement)node).FindElementIndex();
                        builder.Insert(0, "/" + node.Name + "[" + index + "]");
                        node = node.ParentNode;
                        break;
                    case XmlNodeType.Document:
                        return builder.ToString();
                    default:
                        throw new ArgumentException("Only elements and attributes are supported");
                }
            }

            throw new ArgumentException("Node was not in a document");
        }

        /// <summary>
        /// Gets a count of how many times a given string occurs in the base string.
        /// </summary>
        [Extension]
        public static int CountStringOccurance(this string input, string identifier)
        {
            var count = 0;
            while (input.Contains(identifier))
            {
                input = input.Remove(0, input.IndexOf(identifier, StringComparison.Ordinal) + identifier.Length);
                count += 1;
            }

            return count;
        }

        /// <summary>
        /// Gets an array of occurring internal strings that are between a left and right string.
        /// </summary>
        [Extension]
        public static string[] GetListOfInternalStrings(this string input, string left, string right)
        {
            var list = new List<string>();
            while (input.Contains(left))
            {
                if (input.Contains(left) && input.Contains(right))
                {
                    list.Add(input.GetInternalString(left, right));
                    input = input.Replace(left + list[list.Count - 1] + right, "|" + list[list.Count - 1] + "|");
                }
                else
                {
                    break;
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// Surrounds the base array of strings with a prefix and suffix. Optionally, empty strings will be skipped.
        /// </summary>
        [Extension]
        public static string SurroundJoin(this string[] arr, string prefix, string suffix, bool skipEmpties = false)
        {
            var output = new StringBuilder();
            if (arr != null)
            {
                foreach (var s in arr)
                {
                    if ((skipEmpties && !string.IsNullOrEmpty(s)) || !skipEmpties)
                    {
                        output.Append(prefix + s + suffix);
                    }
                }
            }

            return output.ToString();
        }

        /// <summary>
        /// Gets the first occurrence of an internal string between a left and right string.
        /// </summary>
        [Extension]
        public static string GetInternalString(this string input, string left, string right)
        {
            if (input.Contains(left) && input.Contains(right))
            {
                return input.Substring(
                    input.IndexOf(left, StringComparison.Ordinal) + left.Length,
                    input.IndexOf(right, input.IndexOf(left, StringComparison.Ordinal) + left.Length, StringComparison.Ordinal)
                    - input.IndexOf(left, StringComparison.Ordinal) - left.Length);
            }

            return input;
        }

        [DllImport("mpr.dll", CharSet = CharSet.Ansi, EntryPoint = "WNetGetConnectionA")]
        private static extern int WNetGetConnection(string lpszLocalName, StringBuilder lpszRemoteName, ref int cbRemoteName);

        /// <summary>
        /// Gets the UNC representation of the provided path.
        /// </summary>
        public static string GetUNCPath(string sFilePath)
        {
            // Check before allocating resources...
            if (sFilePath.StartsWith("\\", StringComparison.Ordinal))
            {
                return sFilePath;
            }

            // Now allocate resources for processing
            var allDrives = DriveInfo.GetDrives();
            var driveType = 0;
            var driveLetter = sFilePath.Substring(0, 3);
            var strBldr = new StringBuilder();

            var uncNameBuilder = new StringBuilder(160);

            for (var i = 0; i < allDrives.Length; i += 1)
            {
                if (allDrives[i].Name == driveLetter)
                {
                    driveType = (int)allDrives[i].DriveType;
                    break;
                }
            }

            if (driveType == 4)
            {
                var uncLength = uncNameBuilder.Capacity;
                var ctr = WNetGetConnection(sFilePath.Substring(0, 2), uncNameBuilder, ref uncLength);

                if (ctr == 0)
                {
                    var uncName = uncNameBuilder.ToString().Trim();
                    for (ctr = 0; ctr < uncName.Length; ctr += 1)
                    {
                        var singleChar = uncName[ctr];
                        var asciiValue = (int)singleChar;
                        if (asciiValue > 0)
                        {
                            strBldr.Append(singleChar);
                        }
                        else
                        {
                            break;
                        }
                    }

                    strBldr.Append(sFilePath.Substring(2));
                    return strBldr.ToString();
                }

                return sFilePath;
            }

            return sFilePath;
        }

        public delegate void PathStructureLogEventHandler(object sender, string e);
        public static event PathStructureLogEventHandler PathStructureLog;

        public static void Log(string input)
        {
            PathStructureLog?.Invoke(null, input);
        }

        public static void AddDirectorySecurity(string path, string account, FileSystemRights rights, AccessControlType controlType)
        {
            var dir = new DirectoryInfo(path);
            AddDirectorySecurity(dir, account, rights, controlType);
        }

        public static void AddDirectorySecurity(DirectoryInfo dir, string account, FileSystemRights rights, AccessControlType controlType)
        {
            var sec = dir.GetAccessControl();
            sec.AddAccessRule(new FileSystemAccessRule(account, rights, controlType));
            dir.SetAccessControl(sec);
        }

        public static void RemoveDirectorySecurity(string path, string account, FileSystemRights rights, AccessControlType controlType)
        {
            var dir = new DirectoryInfo(path);
            RemoveDirectorySecurity(dir, account, rights, controlType);
        }

        public static void RemoveDirectorySecurity(DirectoryInfo dir, string account, FileSystemRights rights, AccessControlType controlType)
        {
            var sec = dir.GetAccessControl();
            sec.RemoveAccessRule(new FileSystemAccessRule(account, rights, controlType));
            dir.SetAccessControl(sec);
        }

        /// <summary>
        /// Gets an array of raw variable names from the provided string.
        /// </summary>
        public static string[] GetListOfRawVariables(string input)
        {
            var list = new List<string>();
            while (input.Contains("{") || input.Contains("}"))
            {
                if (input.IndexOf("{", StringComparison.Ordinal) < input.IndexOf("}", StringComparison.Ordinal))
                {
                    input = input.Remove(0, input.IndexOf("{", StringComparison.Ordinal) + 1);
                    list.Add(input.Remove(input.IndexOf("}", StringComparison.Ordinal)));
                    input = input.Remove(0, input.IndexOf("}", StringComparison.Ordinal) + 1);
                }
            }

            return list.ToArray();
        }

        [Extension]
        public static bool IsNullOrEmpty(this object[] objs)
        {
            return !(objs != null && objs.Length >= 0);
        }
    }
}
