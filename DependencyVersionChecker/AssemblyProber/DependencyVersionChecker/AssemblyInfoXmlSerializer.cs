using System;
using System.Collections.Generic;
using System.Xml;

namespace AssemblyProber
{
    /// <summary>
    /// XML serialization utility class for IAssemblyInfo objects.
    /// </summary>
    public static class AssemblyInfoXmlSerializer
    {
        /// <summary>
        /// Serializes a collection of IAssemblyInfo to a XmlDocument.
        /// </summary>
        /// <param name="assemblies">IAssemblyInfo collection to serialize</param>
        /// <returns>New XmlDocument</returns>
        public static XmlDocument SerializeToDocument(this IEnumerable<IAssemblyInfo> assemblies)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode docNode = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(docNode);

            XmlNode assembliesNode = doc.CreateElement("Assemblies");

            foreach (IAssemblyInfo a in assemblies)
            {
                assembliesNode.AppendChild(GetAssemblyNode(a, doc));
            }
            doc.AppendChild(assembliesNode);
            return doc;
        }

        private static XmlNode GetAssemblyNode(IAssemblyInfo a, XmlDocument doc)
        {
            XmlNode assemblyNode = doc.CreateElement("AssemblyInfo");

            XmlElement assemblyFullName = doc.CreateElement("FullName");
            assemblyFullName.AppendChild(doc.CreateTextNode(a.FullName));
            assemblyNode.AppendChild(assemblyFullName);

            XmlElement simpleName = doc.CreateElement("SimpleName");
            simpleName.AppendChild(doc.CreateTextNode(a.SimpleName));
            assemblyNode.AppendChild(simpleName);

            XmlElement version = doc.CreateElement("Version");
            version.AppendChild(doc.CreateTextNode(a.Version.ToString()));
            assemblyNode.AppendChild(version);

            XmlElement culture = doc.CreateElement("Culture");
            culture.AppendChild(doc.CreateTextNode(a.Culture));
            assemblyNode.AppendChild(culture);

            XmlElement fileVersion = doc.CreateElement("FileVersion");
            fileVersion.AppendChild(doc.CreateTextNode(a.FileVersion));
            assemblyNode.AppendChild(fileVersion);

            XmlElement informationalVersion = doc.CreateElement("InformationalVersion");
            informationalVersion.AppendChild(doc.CreateTextNode(a.InformationalVersion));
            assemblyNode.AppendChild(informationalVersion);

            XmlElement description = doc.CreateElement("Description");
            description.AppendChild(doc.CreateTextNode(a.Description));
            assemblyNode.AppendChild(description);

            XmlElement company = doc.CreateElement("Company");
            company.AppendChild(doc.CreateTextNode(a.Company));
            assemblyNode.AppendChild(company);

            XmlElement product = doc.CreateElement("Product");
            product.AppendChild(doc.CreateTextNode(a.Product));
            assemblyNode.AppendChild(product);

            XmlElement trademark = doc.CreateElement("Trademark");
            trademark.AppendChild(doc.CreateTextNode(a.Trademark));
            assemblyNode.AppendChild(trademark);

            XmlElement copyright = doc.CreateElement("Copyright");
            copyright.AppendChild(doc.CreateTextNode(a.Copyright));
            assemblyNode.AppendChild(copyright);

            XmlElement borderName = doc.CreateElement("BorderName");
            borderName.AppendChild(doc.CreateTextNode(a.BorderName));
            assemblyNode.AppendChild(borderName);

            XmlElement publicKeyToken = doc.CreateElement("PublicKeyToken");
            publicKeyToken.AppendChild(doc.CreateTextNode(StringUtils.ByteArrayToHexString(a.PublicKeyToken)));
            assemblyNode.AppendChild(publicKeyToken);

            XmlElement paths = doc.CreateElement("Paths");
            foreach (string p in a.Paths)
            {
                XmlElement path = doc.CreateElement("Path");
                path.AppendChild(doc.CreateTextNode(p));
                paths.AppendChild(path);
            }
            assemblyNode.AppendChild(paths);

            XmlElement dependencies = doc.CreateElement("Dependencies");
            foreach (var pair in a.Dependencies)
            {
                XmlElement reference = doc.CreateElement("Reference");

                reference.SetAttribute("As", pair.Key);

                reference.AppendChild(doc.CreateTextNode(pair.Value.FullName));
                dependencies.AppendChild(reference);
            }
            assemblyNode.AppendChild(dependencies);

            return assemblyNode;
        }

        /// <summary>
        /// Deserialize a XmlDocument into a collection of IAssemblyInfo.
        /// </summary>
        /// <param name="doc">XmlDocument to read</param>
        /// <returns>Collection of new IAssemblyInfo</returns>
        public static IEnumerable<IAssemblyInfo> DeserializeFromDocument(XmlDocument doc)
        {
            Dictionary<string, AssemblyInfo> assemblies = new Dictionary<string, AssemblyInfo>();

            foreach (XmlNode n in doc.DocumentElement)
            {
                AssemblyInfo a = GetInfoFromNode(n);
                assemblies.Add(a.FullName, a);
            }

            foreach (XmlNode n in doc.DocumentElement)
            {
                AssemblyInfo a = assemblies[n["FullName"].FirstChild.Value];

                foreach (XmlNode r in n["Dependencies"])
                {
                    AssemblyInfo d = assemblies[r.FirstChild.Value];
                    string asReference = r.Attributes["As"].Value;
                    a.InternalDependencies.Add(asReference, d);
                }
            }

            return assemblies.Values;
        }

        private static AssemblyInfo GetInfoFromNode(XmlNode n)
        {
            AssemblyInfo a = new AssemblyInfo();

            a.FullName = n["FullName"].FirstChild.Value;
            a.SimpleName = n["SimpleName"].FirstChild.Value;
            a.Version = Version.Parse(n["Version"].FirstChild.Value);

            a.Culture = n["Culture"].FirstChild == null ? String.Empty : n["Culture"].FirstChild.Value;

            a.FileVersion = n["FileVersion"].FirstChild == null ? String.Empty : n["FileVersion"].FirstChild.Value;
            a.InformationalVersion = n["InformationalVersion"].FirstChild == null ? String.Empty : n["InformationalVersion"].FirstChild.Value;
            a.Description = n["Description"].FirstChild == null ? String.Empty : n["Description"].FirstChild.Value;

            a.Company = n["Company"].FirstChild == null ? String.Empty : n["Company"].FirstChild.Value;
            a.Product = n["Product"].FirstChild == null ? String.Empty : n["Product"].FirstChild.Value;
            a.Trademark = n["Trademark"].FirstChild == null ? String.Empty : n["Trademark"].FirstChild.Value;
            a.Copyright = n["Copyright"].FirstChild == null ? String.Empty : n["Copyright"].FirstChild.Value;

            a.PublicKeyToken = n["PublicKeyToken"].FirstChild == null ? new byte[0] : StringUtils.HexStringToByteArray(n["PublicKeyToken"].FirstChild.Value);

            if (n["BorderName"].FirstChild == null || String.IsNullOrEmpty(n["BorderName"].FirstChild.Value))
            {
                a.BorderName = null;
            }
            else
            {
                a.BorderName = n["BorderName"].FirstChild.Value;
            }

            foreach (XmlNode p in n["Paths"])
            {
                a.Paths.Add(p.FirstChild.Value);
            }
            return a;
        }

        /// <summary>
        /// Serializes a collection of IAssemblyInfo to a given XmlWriter.
        /// </summary>
        /// <param name="assemblies">IAssemblyInfo collection to serialize</param>
        /// <param name="w">XmlWriter to use</param>
        public static void SerializeTo(this IEnumerable<IAssemblyInfo> assemblies, XmlWriter w)
        {
            w.WriteStartDocument(true);
            w.WriteStartElement("Assemblies");

            foreach (IAssemblyInfo a in assemblies)
            {
                WriteAssemblyInfo(a, w);
            }

            w.WriteEndElement();
            w.WriteEndDocument();
        }

        private static void WriteAssemblyInfo(IAssemblyInfo a, XmlWriter w)
        {
            w.WriteStartElement("AssemblyInfo");

            w.WriteStartElement("FullName");
            w.WriteValue(a.FullName);
            w.WriteEndElement();

            w.WriteStartElement("SimpleName");
            w.WriteValue(a.SimpleName);
            w.WriteEndElement();

            w.WriteStartElement("Version");
            w.WriteValue(a.Version.ToString());
            w.WriteEndElement();

            w.WriteStartElement("Culture");
            w.WriteValue(a.Culture);
            w.WriteEndElement();

            w.WriteStartElement("FileVersion");
            w.WriteValue(a.FileVersion);
            w.WriteEndElement();

            w.WriteStartElement("InformationalVersion");
            w.WriteValue(a.InformationalVersion);
            w.WriteEndElement();

            w.WriteStartElement("Description");
            w.WriteValue(a.Description);
            w.WriteEndElement();

            w.WriteStartElement("Company");
            w.WriteValue(a.Company);
            w.WriteEndElement();

            w.WriteStartElement("Product");
            w.WriteValue(a.Product);
            w.WriteEndElement();

            w.WriteStartElement("Trademark");
            w.WriteValue(a.Trademark);
            w.WriteEndElement();

            w.WriteStartElement("Copyright");
            w.WriteValue(a.Copyright);
            w.WriteEndElement();

            w.WriteStartElement("BorderName");
            w.WriteValue(a.BorderName);
            w.WriteEndElement();

            w.WriteStartElement("PublicKeyToken");
            w.WriteValue(StringUtils.ByteArrayToHexString(a.PublicKeyToken));
            w.WriteEndElement();

            w.WriteStartElement("Paths");
            foreach (var p in a.Paths)
            {
                w.WriteStartElement("Path");
                w.WriteValue(p);
                w.WriteEndElement();
            }
            w.WriteEndElement();

            w.WriteStartElement("Dependencies");
            foreach (var pair in a.Dependencies)
            {
                IAssemblyInfo d = pair.Value;
                string asReference = pair.Key;

                w.WriteStartElement("Reference");

                w.WriteStartAttribute("As");
                w.WriteValue(asReference);
                w.WriteEndAttribute();

                w.WriteValue(d.FullName);
                w.WriteEndElement();
            }
            w.WriteEndElement();

            w.WriteEndElement();
        }

        /// <summary>
        /// Read and deserialize a collection of IAssemblyInfo from a given XmlReader.
        /// </summary>
        /// <param name="r">XmlReader to use</param>
        /// <returns>New collection of IAssemblyInfo</returns>
        public static IEnumerable<IAssemblyInfo> DeserializeFrom(XmlReader r)
        {
            // Assemblies read by the reader.
            Dictionary<string, AssemblyInfo> assemblies = new Dictionary<string, AssemblyInfo>();

            // Assembly names which still need resolution, with the assemblies that need it.
            Dictionary<string, Dictionary<AssemblyInfo, string>> pendingResolution = new Dictionary<string, Dictionary<AssemblyInfo, string>>();

            while (r.Read())
            {
                if (r.IsStartElement() && r.Name == "AssemblyInfo")
                {
                    ReadAssembly(r.ReadSubtree(), assemblies, pendingResolution);
                }
            }

            return assemblies.Values;
        }

        private static void ReadAssembly(XmlReader r, Dictionary<string, AssemblyInfo> assemblies, Dictionary<string, Dictionary<AssemblyInfo, string>> pendingResolution)
        {
            AssemblyInfo a = new AssemblyInfo();

            a.Culture = String.Empty;
            a.Description = String.Empty;
            a.FileVersion = String.Empty;
            a.InformationalVersion = String.Empty;
            a.Description = String.Empty;
            a.Company = String.Empty;
            a.Product = String.Empty;
            a.Trademark = String.Empty;
            a.Copyright = String.Empty;
            a.BorderName = null;
            a.PublicKeyToken = new byte[0];

            while (r.Read())
            {
                if (r.IsStartElement() && !r.IsEmptyElement)
                {
                    switch (r.Name)
                    {
                        case "FullName":
                            if (r.Read())
                            {
                                if (r.NodeType == XmlNodeType.Attribute && r.Name == "As")
                                {
                                }
                                a.FullName = r.Value;

                                assemblies.Add(a.FullName, a);

                                Dictionary<AssemblyInfo, string> assembliesReferencingThis;
                                if (pendingResolution.TryGetValue(a.FullName, out assembliesReferencingThis))
                                {
                                    // There are assemblies pending this one
                                    foreach (var pair in assembliesReferencingThis)
                                    {
                                        AssemblyInfo parent = pair.Key;
                                        string asRef = pair.Value;
                                        parent.InternalDependencies.Add(asRef, a);
                                    }

                                    // Now that they're fixed, we can remove the pending references
                                    pendingResolution.Remove(a.FullName);
                                }
                            }
                            break;

                        case "SimpleName":
                            if (r.Read())
                            {
                                a.SimpleName = r.Value;
                            }
                            break;

                        case "Version":
                            if (r.Read())
                            {
                                Version v;
                                if (Version.TryParse(r.Value, out v))
                                    a.Version = v;
                            }
                            break;

                        case "Culture":
                            if (r.Read())
                            {
                                a.Culture = r.Value;
                            }
                            break;

                        case "FileVersion":
                            if (r.Read())
                            {
                                a.FileVersion = r.Value;
                            }
                            break;

                        case "InformationalVersion":
                            if (r.Read())
                            {
                                a.InformationalVersion = r.Value;
                            }
                            break;

                        case "Description":
                            if (r.Read())
                            {
                                a.Description = r.Value.Replace("\n", "\r\n");
                            }
                            break;

                        case "Company":
                            if (r.Read())
                            {
                                a.Company = r.Value;
                            }
                            break;

                        case "Product":
                            if (r.Read())
                            {
                                a.Product = r.Value;
                            }
                            break;

                        case "Trademark":
                            if (r.Read())
                            {
                                a.Trademark = r.Value;
                            }
                            break;

                        case "Copyright":
                            if (r.Read())
                            {
                                a.Copyright = r.Value.Replace("\n", "\r\n");
                            }
                            break;

                        case "BorderName":
                            if (r.Read())
                            {
                                string value = r.Value;
                                if (String.IsNullOrEmpty(value))
                                    a.BorderName = null;
                                else
                                    a.BorderName = value;
                            }
                            break;

                        case "PublicKeyToken":
                            if (r.Read())
                            {
                                a.PublicKeyToken = StringUtils.HexStringToByteArray(r.Value);
                            }
                            break;

                        case "Paths":
                            while (r.Read())
                            {
                                if (r.IsStartElement() && r.Name == "Path")
                                {
                                    if (r.Read())
                                    {
                                        a.Paths.Add(r.Value);
                                    }
                                }
                                else if (r.NodeType == XmlNodeType.EndElement && r.Name == "Paths")
                                {
                                    break;
                                }
                            }
                            break;

                        case "Dependencies":
                            while (r.Read())
                            {
                                if (r.IsStartElement() && r.Name == "Reference")
                                {
                                    string referenceName = r.GetAttribute("As");
                                    if (r.Read())
                                    {
                                        string targetAssemblyName = r.Value;
                                        AssemblyInfo d;
                                        if (assemblies.TryGetValue(targetAssemblyName, out d))
                                        {
                                            // Already resolved
                                            a.InternalDependencies.Add(referenceName, d);
                                        }
                                        else
                                        {
                                            // Not resolved yet
                                            Dictionary<AssemblyInfo, string> thisPendingResolution;

                                            if (!pendingResolution.TryGetValue(targetAssemblyName, out thisPendingResolution))
                                            {
                                                thisPendingResolution = new Dictionary<AssemblyInfo, string>();
                                                pendingResolution.Add(targetAssemblyName, thisPendingResolution);
                                            }

                                            thisPendingResolution.Add(a, referenceName);
                                        }
                                    }
                                }
                                else if (r.NodeType == XmlNodeType.EndElement && r.Name == "Dependencies")
                                {
                                    break;
                                }
                            }
                            break;
                    }
                }
                else if (r.NodeType == XmlNodeType.EndElement && r.Name == "AssemblyInfo")
                {
                    return;
                }
            }
        }
    }
}