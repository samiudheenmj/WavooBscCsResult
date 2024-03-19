using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Hasee
{
    public partial class _Default : Page
    {
		private const string WhitelistedTableClass = "whitelisted-table";
		protected void Page_Load(object sender, EventArgs e)
        {
			PostData();
		}

        private void PostData()
        {
			var cache = MemoryCache.Default;
			var tableCache = (string)cache["ResultTable"];

			if (!string.IsNullOrEmpty(tableCache))
			{
				// do nothing
			}
			else
			{
				List<Student> students = new List<Student>();
				for (int i = 201; i <= 240; i++)
				{
					var client = new HttpClient();
					var values = new Dictionary<string, string>
				  {
					  { "RegisterNo", "20212241506"+i }
				  };

					var content = new FormUrlEncodedContent(values);
					var result = client.PostAsync("http://www.ugresult2023n.msuexamresult.in/print_result.php", content);
					var res = result.Result.Content.ReadAsStringAsync().Result;
					res = res.Replace("<table width=\"100%\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">", "<table class='" + WhitelistedTableClass + "' width=\"100%\" border=\"1\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#FFFFFF\">");

					//Response.Write(res);
					if (res.Contains(WhitelistedTableClass))
					{
						HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
						doc.LoadHtml(res);

						var name = doc.DocumentNode.SelectNodes("//label[@id=\"candidatename\"]").FirstOrDefault().InnerText;
						var tbl = doc.DocumentNode.SelectNodes("//table[@class='" + WhitelistedTableClass + "']").FirstOrDefault();
						var ocount = 0;
						var generalNode = tbl.SelectNodes("tr")[1];
						var generalGrade = generalNode.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
						var generalGP = GetGP(generalGrade);
						ocount = generalGrade == "O" ? ocount + 1 : ocount;

						var elecNode = tbl.SelectNodes("tr")[2];
						var elecGrade = elecNode.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
						var elecGP = GetGP(elecGrade);
						ocount = elecGrade == "O" ? ocount + 1 : ocount;

						var rdbmsNode = tbl.SelectNodes("tr")[3];
						var rdbmsGrade = rdbmsNode.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
						var rdbmsGP = GetGP(rdbmsGrade);
						ocount = rdbmsGrade == "O" ? ocount + 1 : ocount;

						var dccnNode = tbl.SelectNodes("tr")[4];
						var dccnGrade = dccnNode.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
						var dccnGP = GetGP(dccnGrade);
						ocount = dccnGrade == "O" ? ocount + 1 : ocount;

						var phpNode = tbl.SelectNodes("tr")[5];
						var phpGrade = phpNode.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
						var phpGP = GetGP(phpGrade);
						ocount = phpGrade == "O" ? ocount + 1 : ocount;

						var cmcsp5Node = tbl.SelectNodes("tr")[6];
						var cmcsp5Grade = cmcsp5Node.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
						var cmcsp5GP = GetGP(cmcsp5Grade);
						ocount = cmcsp5Grade == "O" ? ocount + 1 : ocount;

						var cmcsp6Node = tbl.SelectNodes("tr")[7];
						var cmcsp6Grade = cmcsp6Node.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
						var cmcsp6GP = GetGP(cmcsp6Grade);
						ocount = cmcsp6Grade == "O" ? ocount + 1 : ocount;

                        var cmcsp7Node = tbl.SelectNodes("tr")[8];
                        var cmcsp7Grade = cmcsp7Node.SelectNodes("td")[2].SelectNodes("font").First().InnerText.Trim().Replace("&nbsp;", "");
                        var cmcsp7GP = GetGP(cmcsp7Grade);
                        ocount = cmcsp7Grade == "O" ? ocount + 1 : ocount;

                        double avg = Math.Round(((generalGP + elecGP + rdbmsGP + dccnGP + phpGP + cmcsp5GP + cmcsp6GP + cmcsp7GP) / 8), 2);
						double avgME = Math.Round(((elecGP + rdbmsGP + dccnGP + phpGP + cmcsp5GP + cmcsp6GP + cmcsp7GP) / 7), 2);

						Student student = new Student()
						{
							Name = name,
							General = generalGrade,
							Elective = elecGrade,
							RDBMS = rdbmsGrade,
							DCCN = dccnGrade,
							PHP = phpGrade,
							CMCSP5 = cmcsp5Grade,
							CMCSP6 = cmcsp6Grade,
							CMCSP7 = cmcsp7Grade,
							O_Count = ocount,
							Average = avg,
							AverageME = avgME,
							RollNo = "20212241506" + i
						};

						students.Add(student);
					}
				}

				cache["ResultTable"] = GetMyTable(students.OrderByDescending(x => x.AverageME), x => x.Name, x => x.RollNo, x => x.General, x => x.Elective, x => x.RDBMS, x => x.DCCN, x => x.PHP, x => x.CMCSP5, x => x.CMCSP6, x => x.CMCSP7, x => x.O_Count, x => x.Average, x => x.AverageME); // add
				tableCache = (string)cache["ResultTable"];
			}

			Response.Write(tableCache);
		}

		public static string GetMyTable<T>(IEnumerable<T> list, params Func<T, object>[] fxns)
		{
			var allList = list.OrderByDescending(x => fxns[11](x));
			StringBuilder sb = new StringBuilder();
			sb.Append("<table border=\"1\" style=\"padding-left:15px\">\n");
			sb.Append("<tr>\n<td><b>Name</b></td>");
			sb.Append("<td><b>RollNo.</b></td>");
			sb.Append("<td><b>PD</b></td>");
			sb.Append("<td><b>Elec</b></td>");
			sb.Append("<td><b>RDBMS</b></td>");
			sb.Append("<td><b>DCCN</b></td>");
			sb.Append("<td><b>PHP</b></td>");
			sb.Append("<td><b>CMCSP5</b></td>");
			sb.Append("<td><b>CMCSP6</b></td>");
            sb.Append("<td><b>CMCSP7</b></td>");
            sb.Append("<td><b>O Count</b></td>");
			sb.Append("<td><b>Average (All)</b></td>");
			sb.Append("<td><b>Average (Main & Elec)</b></td>");
			sb.Append("<td><b>Rank (Main & Elec)</b></td>");
			sb.Append("<td><b>Rank (Overall)</b></td></tr>\n");
			
			var rank = 1;
			foreach (var item in list)
			{
				sb.Append("<tr>\n");
				foreach (var fxn in fxns)
				{
					sb.Append("<td>");
					sb.Append(fxn(item));
					sb.Append("</td>");
				}
				sb.Append("<td>");
				sb.Append(rank);
				sb.Append("</td>");
				var entry = allList.Where(x => fxns[1](x) == fxns[1](item)).FirstOrDefault();
				var allRank = allList.ToList().IndexOf(entry) + 1;
				sb.Append("<td>");
				sb.Append(allRank);
				sb.Append("</td>");
				rank++;
				sb.Append("</tr>\n");
			}
			sb.Append("</table>");

			return sb.ToString();
		}

		private double GetGP(string grade)
		{
			double gp = 0;
			switch (grade)
			{
				case "O":
					gp = 10;
					break;
				case "A+":
					gp = 9;
					break;
				case "A":
					gp = 8;
					break;
				case "B+":
					gp = 7;
					break;
				case "B":
					gp = 6;
					break;
				case "C":
					gp = 5;
					break;
				default:
					gp = 0;
					break;
			}
			return gp;
		}
	}

	public class Student
	{
		public string Name { get; set; }
		public string General { get; set; }
		public string Elective { get; set; }
		public string RDBMS { get; set; }
		public string DCCN { get; set; }
		public string PHP { get; set; }
		public string CMCSP5 { get; set; }
		public string CMCSP6 { get; set; }
        public string CMCSP7 { get; set; }
        public int O_Count { get; set; }
		public double Average { get; set; }
		public double AverageME { get; set; }
		public string RollNo { get; set; }
	}
}