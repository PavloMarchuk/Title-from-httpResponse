using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WebUi.Models;
using System.Threading;

namespace WebUi.Controllers
{
    public class TitleHttpController : Controller
    {

		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult Create( ReturnedTextViewModel URLs)
		{
			if (ModelState.IsValid)
			{		
				

				return RedirectToAction("Answers", "TitleHttp", new { str = URLs.Result });
			}
			return View(URLs);
		}

		public IActionResult Answers(string str = "")
		{
			List<RequestStateViewModel> model = new List<RequestStateViewModel>();

			// ділимо сплітом на рядки
			string[] strArr = str.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
			List<string> strList = strArr.ToList();


			foreach (string item in strArr)
			{
				// в новий VM зразу записуємо рядок URL і додаємо VM в модель
				RequestStateViewModel requestStateVM = new RequestStateViewModel() { UrlText = item };
				model.Add(requestStateVM);


				try
				{
					Uri httpSite;
					// перевірка на Uri
					try
					{
						httpSite = new Uri(item);
					}
					catch (Exception)
					{
						requestStateVM.UrlStatus = $"Uri Error";
						requestStateVM.UrlTitle = "";
						continue;
					}

					WebRequest request = WebRequest.Create(httpSite);

					using (WebResponse response = request.GetResponse())
					{
						HttpWebRequest reqFP = (HttpWebRequest)HttpWebRequest.Create(item);
						HttpWebResponse rspFP = (HttpWebResponse)reqFP.GetResponseAsync().Result;

						requestStateVM.UrlStatus = $"{ ((int)rspFP.StatusCode)} - {rspFP.StatusCode}";

						using (Stream stream = response.GetResponseStream())
						{
							using (StreamReader reader = new StreamReader(stream))
							{
								//.Console.WriteLine(reader.ReadToEnd());

								string source = reader.ReadToEnd();
								requestStateVM.UrlTitle = Regex.Match(source, @"\<title\b[^>]*\>\s*(?<Title>[\s\S]*?)\</title\>", RegexOptions.IgnoreCase).Groups["Title"].Value;
							}
						}
					}
				}


				catch (WebException ex)
				{
					// отримаємо статус WebBинятку
					WebExceptionStatus status = ex.Status;

					if (status == WebExceptionStatus.ProtocolError)
					{
						HttpWebResponse httpResponse = (HttpWebResponse)ex.Response;
						requestStateVM.UrlStatus = $"{(int)httpResponse.StatusCode} - {httpResponse.StatusCode}";
					}
				}
				catch (Exception ex)
				{
					requestStateVM.UrlStatus = $"{ex.GetType()}";

				}
				//WebClient x = new WebClient();
			}
			return PartialView(model);
		
		}

		private void RespCallback(IAsyncResult ar)
		{
			throw new NotImplementedException();
		}
	}
}