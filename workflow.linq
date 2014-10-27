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

public class MoneyBin {
	public MoneyBin(int money) {
		Money = money;
	}
	public int Money {get;set;}
}

async void Main()
{
	string baseAddress = "http://localhost:8090/";
	var cache = MemoryCache.Default;
	var claim = new InsuranceClaim(1);
	cache.Add("claim1", claim, DateTime.MaxValue);
	cache.Add("cash", new MoneyBin(100000), DateTime.MaxValue);
	
	
	claim.Owner = "Bjartnes";
	claim.RequestAmount = 10000;
	claim.SendToEvaluation();
	claim.SetAcceptedAmount(8000, "Bjartwolf");
	//claim.AcceptAmount();
	claim.MakeComplaint();
	//claim.ProcessComplaint(9000, "BEB");
	claim.AcceptAmount();
	claim.AcceptAmount();
claim.AcceptAmount();

	cache.Dump();
/*	using (var app = WebApp.Start<Startup>(url: baseAddress))
	{
		"Workflow engine started".Dump();
		Console.ReadLine();
	}
*/
}


public class InsuranceClaim {
	public readonly int Id;
	public string Owner {get;set;}
	public int ApprovedAmount {get;set;}
	public int? RequestAmount {get;set;}
	public int? PayedAmount {get;set;}
	public string Approver {get; set;}
	public bool? forApproval {get;set;}
	public bool IsEvaluted {get;set;}
	public bool IsAccepted {get;set;}
	public bool IsCompleted {get;set;}	
	public bool HasComplained {get; set;}
    // ALWAYS CHECK FOR CORRECT STATE IN METHOD BEFORE PROCESS PAYMENT
	public InsuranceClaim (int id) {
		Id = id;
	}
	
	public void SendToEvaluation() {
	    if (RequestAmount == null) return;
		forApproval = true;
	}
	
	public void AcceptAmount() {
		if (IsCompleted) return;
		IsAccepted = true;
		MakePayment();
		SetPaymentComplete();
	}
	
	/// <summary>
	/// The SetPaymentComplete method.
	/// Sets the completed state completed.
	/// </summary>
	/// <parameters>
	/// DateTime completedDate
	/// </parameters>
	/// <author>
	/// cx\beb
	/// </author>
	/// <returns>
	/// void
	/// </returns>
	/// <Last modified>
	/// 23.91.29
	/// </Last modified>
	public void SetPaymentComplete() {
		if (HasComplained) return;
		IsCompleted = true;
	}
	
	private void MakePayment() {
	   if (IsCompleted) {
	   	   throw new Exception("Should not accept twice!");
	   }
	   var moneyBin = (MoneyBin)MemoryCache.Default.Get("cash");
	   moneyBin.Money = moneyBin.Money - ApprovedAmount;
	   PayedAmount = ApprovedAmount;
	}

	public void ProcessComplaint (int? newAmount, string approver) {
		Approver = approver;
		if (newAmount.HasValue) {
			ApprovedAmount = newAmount.Value;
		}
		HasComplained = false;
	}

	public void MakeComplaint() {
		HasComplained = true;	
	}
	
	public void SetAcceptedAmount(int amount, string approver) {
		ApprovedAmount = amount;
		Approver = approver;
		IsEvaluted = true;
	}
		
	public void approve(string password) {
		if (password != "admin") return;
	}

}

public class InsuranceClaimController : ApiController
{
	private InsuranceClaim _insuranceClaim;
	public InsuranceClaimController () {
	   _insuranceClaim = MemoryCache.Default.Get("claim1") as InsuranceClaim;
	}

   [Route("approve/{password}")]
   public InsuranceClaim GetApprove(string password)
   {
       _insuranceClaim.approve(password);
	   return _insuranceClaim;
   }

   [Route("sendForComments")]
   public InsuranceClaim GetSendForComments()
   {
//   	   _insuranceClaim.sendForComments();
       return _insuranceClaim;
   }

   [Route("returnFromCommenting")]
   public InsuranceClaim GetReturnFromCommenting()
   {
//   	   _insuranceClaim.returnFromCommenting();
       return _insuranceClaim;
   }

   [Route("sendForApproval")]
   public InsuranceClaim GetSendForApproval()
   {
//   	   _insuranceClaim.sendForApproval();
       return _insuranceClaim;
   }

   [Route("state")]
   public void GetState()
   {
//   	   if (_insuranceClaim.IsApproved) {
    }
	
   [Route("")]
   public HttpResponseMessage GetResult()
   {
       var response = new HttpResponseMessage(HttpStatusCode.OK)
       {
           Content = new StringContent(File.ReadAllText(@"c:\depot_git\workflow\index.html"))
       };
       response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/html");
       return response;
   }
   [Route("angular.min.js")]
   public HttpResponseMessage GetAngular()
   {
       var response = new HttpResponseMessage(HttpStatusCode.OK)
       {
           Content = new StringContent(File.ReadAllText(@"C:\depot_git\ng-demos\ng-1.3 playground\scripts\angular.min.js"))
       };
      // response.Content.Headers.ContentType = new MediaTypeHeaderValue("");
       return response;
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