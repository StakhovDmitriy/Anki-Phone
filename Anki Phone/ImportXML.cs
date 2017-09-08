using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Anki.Tables;
using System.Xml;
using Windows.Storage;
using Windows.Storage.Pickers;
using System.IO;

namespace Anki
{
    class ImportXML
    {
        public async static void Import()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add(".xml");
            XmlDocument xDoc = new XmlDocument();
            StorageFile xmlFile = await openPicker.PickSingleFileAsync();
            if (xmlFile != null)
            {
                xDoc.Load(await (xmlFile).OpenStreamForReadAsync());
                ForceXML(xDoc);
            }
            else System.Diagnostics.Debug.WriteLine("FileIsNull");
        }
        private static async void ForceXML(XmlDocument xDoc)
        {
            int UnitID = -1, LessonID = -1;
            XmlElement unitNode = xDoc.DocumentElement;
            //Внешний узел должен быть курсом
            if (unitNode.Name.ToLower() == "unit")
            {
                if (unitNode.Attributes.Count > 0)
                {
                    XmlNode attr = unitNode.Attributes.GetNamedItem("name");
                    if (attr != null)
                    {
                        List<UnitDB> units = await DBHelper.GetItemsAsync<UnitDB>();
                        UnitDB UnitInTable = null;
                        foreach (UnitDB unit in units)
                        {
                            if (unit.Name == attr.Value)
                            {
                                UnitInTable = unit;
                                break;
                            }
                        }
                        if (UnitInTable != null)
                            await DBHelper.SaveItemAsync(UnitInTable);
                        else await DBHelper.SaveItemAsync(new UnitDB { Name = attr.Value });
                        units = await DBHelper.GetItemsAsync<UnitDB>();
                        foreach (UnitDB unit in units)
                        {
                            if (unit.Name == attr.Value)
                            {
                                UnitID = unit.ID;
                                break;
                            }
                        }
                    }
                    else new xmlException("Course haven't name.");
                }
                else new xmlException("Course haven't name.");
                //Работаем с уроками
                foreach (XmlNode lessonNode in unitNode)
                {
                    if (lessonNode.Attributes.Count > 0)
                    {
                        XmlNode attr = lessonNode.Attributes.GetNamedItem("name");
                        if (attr != null)
                        {
                            await DBHelper.SaveItemAsync(new LessonDB { Name = attr.Value, ParentId = UnitID });
                            foreach (LessonDB lesson in await DBHelper.GetItemsAsync<LessonDB>())
                            {
                                if (lesson.Name == attr.Value && lesson.ParentId == UnitID)
                                {
                                    LessonID = lesson.ID;
                                    break;
                                }
                            }
                        }
                        else new xmlException("Lesson haven't name.");
                    }
                    else new xmlException("Lesson haven't name.");
                    if (lessonNode.Name == "lesson")
                    {
                        foreach (XmlNode LessonComponent in lessonNode)
                        {
                            if (LessonComponent.Name.ToLower() == "dictionary")
                                foreach (XmlNode itemNode in LessonComponent)
                                {
                                    if (itemNode.Name.ToLower() == "item")
                                    {
                                        await DBHelper.SaveItemAsync(new Tables.ItemDB
                                        {
                                            Kanji = itemNode["kanji"].InnerText,
                                            OnReading = itemNode["onReading"].InnerText,
                                            KunReading = itemNode["kunReading"].InnerText,
                                            Meaning = itemNode["meaning"].InnerText,
                                            ParentId = LessonID,
                                            Progress = 0
                                        });
                                    }
                                    else new xmlException("Lesson includes unknown node: " + itemNode.Name);
                                }
                            if (LessonComponent.Name.ToLower() == "readingtest")
                                await DBHelper.SaveItemAsync(new Tables.TestDB
                                {
                                    Type = TestType.ReadingTest,
                                    From = LessonComponent["from"].InnerText,
                                    To = LessonComponent["to"].InnerText,
                                    ParentId = LessonID
                                });
                            if (LessonComponent.Name.ToLower() == "writingtest")
                                await DBHelper.SaveItemAsync(new Tables.TestDB
                                {
                                    Type = TestType.WritingTest,
                                    From = LessonComponent["from"].InnerText,
                                    To = LessonComponent["to"].InnerText,
                                    ParentId = LessonID
                                });
                            
                        }
                    }
                    else new xmlException("Course includes unknown node: " + lessonNode.Name);

                }
                System.Diagnostics.Debug.WriteLine("Successfull");
            }
            else new xmlException("This xml-file is not unit. Get: " + unitNode.Name.ToLower());
        }
        class xmlException
        {
            public xmlException(string message)
            {
                System.Diagnostics.Debug.WriteLine("Error!" + message);
            }
        }
    }
    
}

