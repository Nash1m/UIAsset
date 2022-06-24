using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Nash1m.Tools.Scene
{
    public class SceneReader : MonoBehaviour
    {
        public SceneHierarchy hierarchy;

        [ContextMenu("Test")]
        public void Test()
        {
            ReadSceneFile("TestScene.unity");
            // var allGo = FindObjectsOfType<RectTransform>();
            // foreach (var go in allGo)
            // {
            //     Debug.Log(go.GetInstanceID());
            // }
        }

        public SceneReader(string filePath)
        {
            ReadSceneFile(filePath);
        }

        #region Reading

        private void ReadSceneFile(string path)
        {
            var content = File.ReadAllText(Path.Combine(Application.dataPath, path));

            var objects = GetAllObjects(content);
            var allGameObjects = GetAllGameObjects(objects);
            var rootObjects = GetRootObjects(allGameObjects);
            FillChildrenAndFathers(allGameObjects);
            hierarchy = new SceneHierarchy()
            {
                rootObjects = rootObjects
            };
        }

        private List<HierarchyObject> GetAllObjects(string content)
        {
            var objectsText = content.Split(new[] {"---"}, StringSplitOptions.None);
            var objectsLines = objectsText.Select(x => x.Split('\n')).ToArray();
            var objects = new List<HierarchyObject>();
            for (var i = 1; i < objectsLines.Length; i++)
            {
                var objectLines = objectsLines[i];
                objects.Add(new HierarchyObject(objectLines,
                    new[] {"m_Name", "m_Children", "m_Father", "m_Component"}));
            }

            return objects;
        }

        private List<HierarchyElement> GetAllGameObjects(List<HierarchyObject> allObjects)
        {
            var f1 = "{fileID: ".Length;
            var f2 = "{fileID: 0}".Length - f1 - 1;
            var c1 = "{fileID: ".Length;
            var c2 = "{fileID: 563816513}".Length - c1 - 1;
            return allObjects.Where(x => x.type == "GameObject")
                .Select(x =>
                {
                    var father = x.GetField("m_Father");
                    return new HierarchyElement
                    {
                        children = x.GetField("m_Children").values.Select(y => new HierarchyElement()
                        {
                            id = y.Substring(c1, c2)
                        }).ToList(),
                        components = new List<string>(),
                        father = father.values[0] == "{fileID: 0}"
                            ? null
                            : new HierarchyElement()
                            {
                                id = father.values[0].Substring(f1, f2)
                            },
                        id = x.id,
                        name = x.GetField("m_Name").values[0]
                    };
                }).ToList();
        }

        private List<HierarchyElement> GetRootObjects(List<HierarchyElement> allGameObjects)
        {
            return allGameObjects
                .Where(x => x.father == null)
                .ToList();
        }

        private void FillChildrenAndFathers(List<HierarchyElement> allElements)
        {
            foreach (var hierarchyElement in allElements)
            {
                hierarchyElement.children = hierarchyElement.children
                    .Select(x => allElements.FirstOrDefault(y => y.id == x.id))
                    .ToList();
            }
            foreach (var hierarchyElement in allElements)
            {
                hierarchyElement.father = allElements.FirstOrDefault(y => y.id == hierarchyElement.father.id);
            }
        }

        #endregion
    }

    [System.Serializable]
    public class SceneHierarchy
    {
        public List<HierarchyElement> rootObjects;
    }

    [System.Serializable]
    public class HierarchyElement
    {
        public string name;
        public string id;
        public List<string> components;

        public HierarchyElement father;
        public List<HierarchyElement> children;
    }


    [System.Serializable]
    public class HierarchyObject
    {
        public string id;
        public string type;
        public List<HierarchyField> fields = new List<HierarchyField>();
        public readonly string[] requiredFields;

        public HierarchyObject(string[] lines, string[] requiredFields)
        {
            this.requiredFields = requiredFields;
            id = lines[0].Split('&')[1];
            type = new string(lines[1].Take(lines[1].Length - 1).ToArray());
            var length = lines.Length;
            HierarchyField field = null;

            for (var i = 2; i < length; i++)
            {
                var line = lines[i];

                if (line.Length <= 3) continue;
                if (line.Substring(0, 3) != "  -")
                {
                    if (field is { })
                        fields.Add(field);

                    var nameSplitterIndex = line.IndexOf(':');
                    if (nameSplitterIndex < 0) continue;
                    field = new HierarchyField();
                    field.name = line.Substring(2, nameSplitterIndex - 2);
                    if (!FilterField(field.name))
                    {
                        field = null;
                        continue;
                    }

                    if (line.Length > nameSplitterIndex + 1)
                    {
                        field.values.Add(line.Substring(nameSplitterIndex + 2,
                            line.Length - nameSplitterIndex - 2));
                        fields.Add(field);
                        field = null;
                    }
                }
                else
                {
                    if (field is null) continue;
                    field.values.Add(line.Substring(4, line.Length - 4));
                }
            }
        }


        public HierarchyField GetField(string name)
        {
            return fields.FirstOrDefault(y => y.name == name);
        }

        private bool FilterField(string field)
        {
            return requiredFields.Contains(field);
        }
    }


    [System.Serializable]
    public class HierarchyField
    {
        public string name;
        public List<string> values = new List<string>();
    }
}