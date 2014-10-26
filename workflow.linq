<Query Kind="Program">
  <Connection>
    <ID>bfa572dc-24e2-4dc6-affd-580c58933830</ID>
    <Persist>true</Persist>
    <Server>localhost</Server>
    <Database>MyFastDB</Database>
    <ShowServer>true</ShowServer>
  </Connection>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <NuGetReference>Microsoft.AspNet.WebApi.OwinSelfHost</NuGetReference>
  <Namespace>Microsoft.Owin.Hosting</Namespace>
  <Namespace>Owin</Namespace>
  <Namespace>System</Namespace>
  <Namespace>System.Collections.Generic</Namespace>
  <Namespace>System.IO</Namespace>
  <Namespace>System.Linq</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Runtime.Caching</Namespace>
  <Namespace>System.Text</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web.Http</Namespace>
</Query>

async void Main()
{
	string baseAddress = "http://localhost:8090/";
	var cache = MemoryCache.Default;
	cache.Add("doc", new Document(1), DateTime.Now.AddDays(1));
	using (var app = WebApp.Start<Startup>(url: baseAddress))
	{
		"Workflow engine started".Dump();
		Console.ReadLine();
	}
}

public class Document {
	public readonly int Id;
	public bool isDraft {get;set;}
	public bool isApproved {get;set;}
	
	public Document (int id) {
		isDraft = true;
		Id = id;
	}
		
	public void approve(string password) {
		if (password != "admin") return;
		isApproved = true;
	}

    /// <summary>
    /// The send for approval method.
    /// Send document for approval by setting
	/// documented to for approval state.
	/// Best-practice documenation should make it look like code is 
	/// documented, obscure the important details with
	/// crap comments and hide the rest of the code from the developer.
    /// </summary>
    /// <returns>void</returns>
	/// <remarks>None</remarks>
	/// <writtenBy>Bjartwolf</writtenBy>
	/// <writtenAt>01-02-2012</writtenAt>

	public void sendForApproval() {
		isDraft = false;
	}

	public void returnFromCommenting() {
		isDraft = false;
		isApproved = false;
	}
		
	public void sendForComments() {
		// Both means commentmode
		isDraft = true;
		isApproved = true;
	}
}

public class DocumentController : ApiController
{
	private Document _doc;
	public DocumentController () {
	   _doc = MemoryCache.Default.Get("doc") as Document;
	}

   [Route("approve/{password}")]
   public Document GetApprove(string password)
   {
       _doc.approve(password);
	   return _doc;
   }

   [Route("sendForComments")]
   public Document GetSendForComments()
   {
   	   _doc.sendForComments();
       return _doc;
   }

   [Route("returnFromCommenting")]
   public Document GetReturnFromCommenting()
   {
   	   _doc.returnFromCommenting();
       return _doc;
   }

   [Route("sendForApproval")]
   public Document GetSendForApproval()
   {
   	   _doc.sendForApproval();
       return _doc;
   }

   [Route("state")]
   public string GetState()
   {
   	   if (_doc.isApproved) {
	   		if (_doc.isDraft) {
	   			return "Sent to collegue for comments";
			} else {
	   			return "Approved";
	   		}
		}
	   if (_doc.isDraft) {
	   		return "Draft";
	   }
	   return "For Approval";
    }
}
	

public class Startup 
{ 
   public void Configuration(IAppBuilder appBuilder) 
   { 
       HttpConfiguration config = new HttpConfiguration();
       config.MapHttpAttributeRoutes();
       appBuilder.UseWebApi(config); 
   } 
}