using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using ExcelDataReader;
using Newtonsoft.Json;

namespace FlingoDesktop
{
    public class ret
    {
        public string catalogtree;
        public string course_id;
        public string create_time;
        public string course_name;
        public string update_time;


    }
    class element
    {
        public string cat_id;
        public string cat_name;
        public bool isfather;
        public string[] sons;
        public new string ToString
        {
            get
            {
                string s = "";
                if (this.isfather)
                    foreach (var son in this.sons)
                    {
                        s += " " + son;
                    }
                s += " " + this.cat_id + " " + this.cat_name + " " + this.isfather.ToString();
                return s;
            }
        }
    }
    class row
    {
        public string vid { set; get; }
        public string item { set; get; }
        public string catId { get; set; }
        public int chapter { get; set; }
        public int section { get; set; }
        public int segment { get; set; }
        public static row fromCsv(string line)
        {
            Console.WriteLine(line);
            string[] vals = line.Split(',');
            row r = new row();
            r.vid = Convert.ToString(vals[0]);
            r.item = Convert.ToString(vals[1]);
             var cat_item= r.item;
            var match_ret = Regex.Matches(r.item, @"\d+\-\d+\-\d+\-\d+");
            if (match_ret.Count > 0)
                cat_item = match_ret[0].Value;
            var items = cat_item.Split('-');
            if (items.Length >= 4)
            {
                try
                {
                    r.catId = items[0];
                    r.chapter = int.Parse(items[1]);
                    r.section = int.Parse(items[2]);
                    r.segment = int.Parse(items[3]);
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return r;
        }
    }


    class catalog
    {
        public List<chapter> catalogtree;
    }
    class chapter
    {
        public int id;
        public string label;
        public List<section> children;

    }
    class section
    {
        public int id;
        public string label;
        public bool type;
        public List<segment> children;

    }
    class segment
    {
        public int id;
        public string label;
        public string vid;
        public bool type;
        public bool finished;
    }
    class ProcessCsv
    {

        public static List<element> readxls(string dir)
        {
            //var dir = "C:\\Users\\23303\\Desktop\\轩嵩教育目录20200116\\轩嵩教育目录20200116\\00009coursecat_202001161900.xlsx";
            FileStream fileStream = File.Open(dir, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var stream = fileStream;
            // Auto-detect format, supports:
            //  - Binary Excel files (2.0-2003 format; *.xls)
            //  - OpenXml Excel files (2007 format; *.xlsx)
            IExcelDataReader excelDataReader = ExcelReaderFactory.CreateReader(stream);
            var reader = excelDataReader;

            var result = reader.AsDataSet(new ExcelDataSetConfiguration()
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                {
                    UseHeaderRow = true
                }
            });
            var dataTable = result.Tables[0];
            List<element> elements = new List<element>();

            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                element e = new element
                {
                    cat_id = dataTable.Rows[i][0].ToString(),
                    cat_name = dataTable.Rows[i][3].ToString(),
                    isfather = dataTable.Rows[i][5].ToString().Equals("0"),
                    sons = dataTable.Rows[i][6].ToString().Trim().Split('|')
                };
                elements.Add(e);
            }
            return elements;
        }
        static List<row> getRows(string dir)
        {

            List<row> values = File.ReadAllLines(dir, Encoding.GetEncoding("gbk"))
                                         .Skip(1)
                                         .Select(v => row.fromCsv(v))
                                         .ToList();
            List<row> validrows = new List<row>();
            // filter unvalid cat
            foreach (row r in values)
            {
                //  var regex = new Regex(@"\d+\-\d+\-\d+\-\d+");
                var regex = new Regex(@"\d+\-\d+\-\d+\-\d+");
                var ret = regex.Matches(r.item).Count;
                if (ret != 0)
                {
                    validrows.Add(r);
                }
            }
            return validrows.OrderBy(o => o.catId).ThenBy(o => o.chapter).ThenBy(o => o.section).ThenBy(o => o.segment).ToList();
        }
        static List<row> getCats(List<row> rows)
        {
            var cats = rows.GroupBy(o => o.catId).Select(o => o.First()).ToList();
            return cats;
        }
        static List<row> getChaptersByCourseId(List<row> rows, string course_id)
        {
            var chapters = rows.Where(o => o.catId.Equals(course_id)).
                GroupBy(o => o.chapter).Select(o => o.First()).ToList();

            return chapters;
        }
        static List<row> getSectionsByCIdCh(List<row> rows, string course_id, row chapter)
        {
            var sections = rows.Where(o => o.catId.Equals(course_id) && o.chapter == chapter.chapter).
                GroupBy(o => o.section).Select(o => o.First()).ToList();

            return sections;
        }
        static List<row> getSegmentsByCIdChSec(List<row> rows, string course_id, row chapter, row section)
        {
            var segments = rows.Where(o => o.catId.Equals(course_id) && o.chapter == chapter.chapter && o.section == section.section).
                GroupBy(o => o.segment).Select(o => o.First()).ToList();
            return segments;
        }

        static List<FileInfo> getAllFiles(string dir)
        {
            DirectoryInfo d = new DirectoryInfo(dir);
            return d.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly).ToList();

        }
        static List<element> getChapterLabels(List<element> elements)
        {
            return elements.Where(o => o.isfather).ToList();
        }
        static List<element> getSectionLabels(List<element> elements, element chapter)
        {
            return elements.Where(o => chapter.sons.Any(o.cat_id.Contains)).ToList();
        }
        static List<element> getSegmentlabels(List<element> elements, element section)
        {
            return elements.Where(o => section.sons.Any(o.cat_id.Contains)).ToList();
        }

       public static List<ret> readCsv(string video_dir,string files_dir,TextBox textBox)
        {
            string log = "";
            string new_line = "\n";
            List<ret> rets = new List<ret>();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            try
            {
               // string dir = "C:\\Users\\23303\\Desktop\\all_videos.csv";
                //string xlsdir = @"C:\Users\23303\Desktop\轩嵩教育目录20200116\轩嵩教育目录20200116";
                List<FileInfo> files = getAllFiles(files_dir);
                List<element> temp_labels = readxls(files[0].FullName);
                List<row> valid_vids = getRows(video_dir);
                List<row> all_courses = getCats(valid_vids);
                foreach (var cat in all_courses)
                {
                    List<FileInfo> tempRet = files.Where(o => o.Name.Contains(cat.catId)).ToList(); ;
                    if (tempRet.Count != 0)
                    {
                        Console.WriteLine(tempRet[0].Name);
                        List<element> allLabels = readxls(tempRet[0].FullName);
                        // ch label process
                        var ch_labels = getChapterLabels(allLabels);
                        //ch label

                        log += new_line + cat.catId;
                       //Console.WriteLine("" + cat.catId);
                        int count = 0;
                        var cat_ = new catalog
                        {
                            catalogtree = new List<chapter>(),
                        };
                        List<row> chapters = getChaptersByCourseId(valid_vids, cat.catId);
                        //get label
                        var ch_list = chapters.ToList();
                        int ch_count = Math.Min(ch_labels.Count(), ch_list.Count());
                        if (ch_list.Count != ch_labels.Count())
                            log+=new_line+ "---缺失章数:" + Math.Abs(ch_list.Count() - ch_labels.Count());
                        for (int i = 0; i < ch_count; ++i)
                        {
                            row ch = ch_list[i];
                            element ch_ele = ch_labels[i];
                            //  Console.WriteLine("--" + ch.chapter);
                            chapter ch_ = new chapter
                            {
                                id = count++,
                                label = ch_ele.cat_name,
                                children = new List<section>()
                            };
                            List<element> sec_labels = getSectionLabels(allLabels, ch_ele);
                            List<row> sections = getSectionsByCIdCh(valid_vids, cat.catId, ch);
                            int sec_count = Math.Min(sec_labels.Count(), sections.Count());
                            if (sec_labels.Count() != sections.Count())
                            {
                                log+=new_line+("---第" + i + "章");
                                log+=new_line+("------缺失节数：" + Math.Abs(sec_labels.Count() - sections.Count()));
                            }
                            for (int j = 0; j < sec_count; ++j)
                            {
                                row sec = sections[j];
                                element sec_ele = sec_labels[j];
                                // Console.WriteLine("----" + sec.section);
                                section sec_ = new section
                                {
                                    id = count++,
                                    label = sec_ele.cat_name,
                                    type = false,
                                    children = new List<segment>()
                                };
                                List<row> segments = getSegmentsByCIdChSec(valid_vids, cat.catId, ch, sec);
                                List<element> seg_labels = getSegmentlabels(allLabels, sec_ele);
                                int seg_count = Math.Min(segments.Count, seg_labels.Count);
                                if (seg_labels.Count() != segments.Count())
                                {
                                    log+=new_line+("-----第" + i + "节");
                                    log+=new_line+("--------缺失小节数：" + Math.Abs(seg_labels.Count() - segments.Count()));
                                }
                                for (int k = 0; k < seg_count; ++k)
                                {
                                    row seg = segments[k];
                                    element seg_ele = seg_labels[k];
                                    // Console.WriteLine("--------" + seg.segment);
                                    segment seg_ = new segment
                                    {
                                        id = count++,
                                        label = seg_ele.cat_name,
                                        vid = seg.vid,
                                        type = true,
                                    };
                                    sec_.children.Add(seg_);
                                }
                                ch_.children.Add(sec_);
                            }
                            cat_.catalogtree.Add(ch_);

                        }
                        /*      using (StreamWriter file = File.CreateText("C:\\Users\\23303\\Desktop\\csharpdemo\\" + cat.catId + ".json"))
                              {
                                  JsonSerializer serializer = new JsonSerializer();
                                  serializer.Serialize(file, cat_);
                              }*/
                        String output = JsonConvert.SerializeObject(cat_);
                        ret temp = new ret
                        {
                            catalogtree = output,
                            course_id = cat.catId,
                            course_name = DateTime.Now.ToString()+"|cid="+cat.catId,
                            create_time= DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                            update_time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        };
                        rets.Add(temp);
                        File.WriteAllText("C:\\Users\\23303\\Desktop\\csharpdemo\\" + cat.catId + ".json",
                                          output);
                    }
                    else
                    {
                        log+=new_line+("--------------------------未匹配到课程");
                        log += new_line + (cat.catId);
                        log += new_line + ("--------------------------未匹配到课程");

                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            textBox.Text+=log;
            return rets;
        }
  
    }
}
