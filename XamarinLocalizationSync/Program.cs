using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XamarinLocalizationSync
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("my name is XamarinLocalizationSync");
            try
            {
                //check and validate parameters
                Dictionary<string, string> paramS = new Dictionary<string, string>();
                foreach (string c in args)
                {
                    var p = c;
                    if (p.StartsWith("/"))
                        p = p.Substring(1);
                    if (p.Contains("="))
                    {
                        var cs = p.Split('=');
                        paramS.Add(cs[0].ToLower(), cs[1]);
                    }
                    else
                        paramS.Add(p.ToLower(), string.Empty);
                }

                foreach (string p in paramS.Keys)
                {
                    if (string.IsNullOrEmpty(paramS[p]))
                        Console.WriteLine("Param:" + p + " is set");
                    else
                        Console.WriteLine("Param:" + p + " : " + paramS[p]);
                }

                if (!paramS.ContainsKey("xmlpath"))
                {
                    Console.WriteLine("parameter xmlPath missing (strings.xml file)");
                    return;
                }

                if (!paramS.ContainsKey("resxpath"))
                {
                    Console.WriteLine("parameter resxPath missing (strings.resx file)");
                    return;
                }

                if (!paramS.ContainsKey("synctoxml") && !paramS.ContainsKey("synctoresx"))
                {
                    Console.WriteLine("at least one parameter must be set: syncToXml | syncToResx");
                    return;
                }

                string xmlPath = paramS["xmlpath"];
                string resxPath = paramS["resxpath"];

                if (!File.Exists(xmlPath))
                {
                    Console.WriteLine("file does not exist: " + xmlPath);
                    return;
                }
                if (!File.Exists(resxPath))
                {
                    Console.WriteLine("file does not exist: " + resxPath);
                    return;
                }

                bool syncToXml = paramS.ContainsKey("synctoxml") && (string.IsNullOrEmpty(paramS["synctoxml"]) || bool.Parse(paramS["synctoxml"]));
                bool syncToResx = paramS.ContainsKey("synctoresx") && (string.IsNullOrEmpty(paramS["synctoresx"]) || bool.Parse(paramS["synctoresx"]));

                //program start
                Console.WriteLine("");

                if (syncToXml)
                {
                    Console.WriteLine("start syncToXml");
                    resXToXml(xmlPath, resxPath);

                    string resFilter = Path.GetFileNameWithoutExtension(resxPath);
                    string resBase = Path.GetDirectoryName(resxPath);
                    string xmlFilter = Path.GetFileName(Path.GetDirectoryName(xmlPath));
                    string xmlBase = Path.GetDirectoryName(Path.GetDirectoryName(xmlPath));

                    foreach (string file in Directory.GetFiles(resBase))
                    {
                        if (Path.GetFileName(file).StartsWith(resFilter) && !file.Equals(resxPath))
                        {
                            string lang = Path.GetFileNameWithoutExtension(file).Substring(resFilter.Length + 1);

                            string xml = Path.Combine(Path.Combine(xmlBase, xmlFilter + "-" + lang), Path.GetFileName(xmlPath));
                            if (File.Exists(xml))
                            {
                                resXToXml(xml, file);
                            }
                        }
                    }
                }

                if (syncToResx)
                {
                    Console.WriteLine("start syncToResx");
                    xmlToResX(xmlPath, resxPath);

                    string xmlFile = Path.GetFileName(xmlPath);
                    string xmlFilter = Path.GetFileName(Path.GetDirectoryName(xmlPath));
                    string xmlBase = Path.GetDirectoryName(Path.GetDirectoryName(xmlPath));

                    foreach (string path in Directory.GetDirectories(xmlBase))
                    {
                        if (path.Substring(path.LastIndexOf('\\') + 1).StartsWith(xmlFilter) && !path.Substring(path.LastIndexOf('\\') + 1).Equals(xmlFilter))
                        {
                            string xml = Path.Combine(path, xmlFile);
                            if (File.Exists(xml))
                            {
                                xml.ToString();
                                string lang = Path.GetFileName(path).Substring(xmlFilter.Length + 1);
                                string resx = resxPath.Replace(".resx", "." + lang + ".resx");
                                if (File.Exists(xml) && File.Exists(resx))
                                    xmlToResX(xml, resx);
                            }
                        }
                    }
                }

                Console.WriteLine("all done");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("error");
                Console.WriteLine(ex.Message);
            }
            Console.Read();
        }

        static bool xmlToResX(string xmlPath, string resxPath)
        {
            bool res = true; //unused but could handle problems in files or a abort-signal

            Console.WriteLine(Path.GetFileName(Path.GetDirectoryName(xmlPath)) + "\\" + Path.GetFileName(xmlPath) + " => " + Path.GetFileName(resxPath));

            //read all values from resx-file
            Dictionary<string, string> orgValues = new Dictionary<string, string>();
            using (ResXResourceReader reader = new ResXResourceReader(resxPath))
            {
                foreach (System.Collections.DictionaryEntry entry in reader)
                {
                    orgValues.Add((string)entry.Key, (string)entry.Value);
                }
            }

            int iTotal = 0;
            int iUpdated = 0;
            int iNew = 0;

            //create new resx-file
            using (ResXResourceWriter resx = new ResXResourceWriter(resxPath))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlPath);
                List<string> doneKeys = new List<string>();

                //import all string-items from xml
                foreach (XmlNode node in doc.DocumentElement.ChildNodes)
                {
                    if (node is XmlElement)
                    {
                        var el = node as XmlElement;
                        if ("string".Equals(el.Name))
                        {
                            string id = el.GetAttribute("name");
                            string value = el.InnerText.Trim().Trim('\"').Trim();
                            string x = el.Value;
                            string y = el.InnerXml;

                            resx.AddResource(id, value);
                            doneKeys.Add(id);

                            iTotal++;
                            iUpdated++;
                            if (!orgValues.ContainsKey(id))
                                iNew++;
                        }
                    }
                }

                //add values that are not contained in xml
                foreach (var org in orgValues)
                {
                    if (!doneKeys.Contains(org.Key))
                    {
                        resx.AddResource(org.Key, org.Value);
                        iTotal++;
                    }
                }
            }

            Console.WriteLine("total " + iTotal + "\tupdated " + iUpdated + "\tnew " + iNew);
            Console.WriteLine("");

            return res;
        }

        static bool resXToXml(string xmlPath, string resxPath)
        {
            bool res = true; //unused but could handle problems in files or a abort-signal

            Console.WriteLine(Path.GetFileName(Path.GetDirectoryName(xmlPath)) + "\\" + Path.GetFileName(xmlPath) + " => " + Path.GetFileName(resxPath));

            //load all Values from resx-file
            Dictionary<string, string> resxValues = new Dictionary<string, string>();
            using (ResXResourceReader reader = new ResXResourceReader(resxPath))
            {
                foreach (System.Collections.DictionaryEntry entry in reader)
                {
                    string key = (string)entry.Key;
                    string value = (string)entry.Value;
                    resxValues.Add(key, "\"" + value + "\"");
                }
            }

            int iTotal = resxValues.Count;
            int iUpdated = 0;
            int iNew = 0;
            int iDeleted = 0;

            //update values in xml
            List<XmlNode> nodesToDelete = new List<XmlNode>();
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);
            List<string> doneKeys = new List<string>();
            foreach (XmlNode node in doc.DocumentElement.ChildNodes)
            {
                if (node is XmlElement)
                {
                    var el = node as XmlElement;
                    if ("string".Equals(el.Name))
                    {
                        string id = el.GetAttribute("name");
                        if (resxValues.ContainsKey(id))
                        {
                            el.InnerText = resxValues[id];
                            el.SetAttribute("sync", "1");
                            resxValues.Remove(id);
                            iUpdated++;
                        }
                        else if (el.HasAttribute("sync"))
                            nodesToDelete.Add(el); //this item was removed in resx-file
                    }
                }
            }

            //add new values
            foreach (string id in resxValues.Keys)
            {
                XmlElement el = doc.CreateElement("string");
                el.SetAttribute("name", id);
                el.SetAttribute("sync", "x");
                el.InnerText = resxValues[id];
                doc.DocumentElement.AppendChild(el);
                iNew++;
            }

            foreach (XmlNode delete in nodesToDelete)
            {
                doc.DocumentElement.RemoveChild(delete);
                iDeleted++;
            }

            //sort nodes
            var sortedNodes = new SortedDictionary<string, XmlNode>();
            foreach (XmlNode n in doc.DocumentElement.ChildNodes)
            {
                string cID = n.Name;
                if (n is XmlElement && (n as XmlElement).HasAttribute("name"))
                {
                    cID += "_" + (n as XmlElement).GetAttribute("name");
                    if (!"string".Equals(n.Name))
                        cID = "ZZZx_" + cID;
                }
                else if (n is XmlComment)
                    continue; //remove comments
                else
                    throw new Exception("Unknown NodeTime:\n" + n.OuterXml);

                while (sortedNodes.ContainsKey(cID))
                    cID += "z"; //is not needet, but so the can be played with id's for other sorting someday

                sortedNodes.Add(cID, n);
            }

            //remove all and add sorted again
            doc.DocumentElement.RemoveAll();
            foreach (XmlNode n in sortedNodes.Values)
                doc.DocumentElement.AppendChild(n);

            //save xml
            doc.Save(xmlPath);

            Console.WriteLine("total " + iTotal + "\tupdated " + iUpdated + "\tnew " + iNew + "\tdeleted " + iDeleted);
            Console.WriteLine("");
            return res;
        }
    }
}