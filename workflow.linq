<Query Kind="Program">
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
	
	public void Withdraw(int amount) {
		if (amount > Money) throw new Exception("Not enough money");
		Money -= amount;
		Money.Dump();
	}
	public int Money {get; private set;}
}

async void Main()
{
	string baseAddress = "http://localhost:8090/";
	var cache = MemoryCache.Default;
	cache.Add("cash", new MoneyBin(100000), DateTime.MaxValue);

	using (var app = WebApp.Start<Startup>(url: baseAddress))
	{
		"Workflow engine started".Dump();
		Console.ReadLine();
	}
}
// <summary>
// This is before we bring subclassing of claims into the picture
// Subclassing if obviously OO done right, so it would make things much more clear
// </summary>
// <remark>
// ALWAYS CHECK FOR CORRECT STATE IN METHOD BEFORE PROCESS PAYMENT
// </remark>
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

	public InsuranceClaim (string owner, int requestAmount) {
		Owner = owner;
		RequestAmount =requestAmount;
		forApproval = true;
		var cache = MemoryCache.Default;
		Id = cache.Count();
		cache.Add("claim"+Id, this, DateTime.MaxValue);
	}
	
	public void Accept() {
		if (IsCompleted) return;
		IsAccepted = true;
		forApproval = false;
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
		if (HasComplained || forApproval.Value) return;
		IsCompleted = true;
		forApproval = false;
	}
	
	private void MakePayment() {
	   if (IsCompleted) {
	   	   throw new Exception("Should not accept twice!");
	   }
	   var moneyBin = (MoneyBin)MemoryCache.Default.Get("cash");
	   moneyBin.Withdraw(ApprovedAmount);
	   PayedAmount = ApprovedAmount;
	}

	public void ProcessComplaint (int? newAmount, string approver, string password) {
		if (password != "admin") return;
		Approver = approver;
		if (newAmount.HasValue) {
			ApprovedAmount = newAmount.Value;
		}
		HasComplained = false;
	}

	public void MakeComplaint() {
		HasComplained = true;
		forApproval = false;
	}
	
	public void SetAcceptedAmount(int amount, string approver, string password) {
		if (password != "admin") return;
		ApprovedAmount = amount;
		Approver = approver;
		IsEvaluted = true;
	}
}

public class InsuranceClaimController : ApiController
{
	private InsuranceClaim loadClaim(int id){
	   return MemoryCache.Default.Get("claim"+id) as InsuranceClaim;
	}

   [HttpGet]
   [Route("reviewcomplaint/{id}/{approver}/{password}/{amount}")]
   public InsuranceClaim ReviewComplaint(int id, string approver, string password, int? amount)
   {
   	   var insuranceClaim = loadClaim(id);
	   insuranceClaim.ProcessComplaint(amount,approver, password);
       return insuranceClaim;
   }

   [HttpGet]
   [Route("review/{id}/{approver}/{password}/{amount}")]
   public InsuranceClaim Review(int id, string approver, string password, int amount)
   {
  	   var insuranceClaim = loadClaim(id);
	   insuranceClaim.SetAcceptedAmount(amount, approver, password);
	   return insuranceClaim;
   }
   
   [HttpGet]
   [Route("complain/{id}/")]
   public InsuranceClaim Complain(int id)
   {
  	   var insuranceClaim = loadClaim(id);
	   insuranceClaim.MakeComplaint();
	   return insuranceClaim;
   }

   
   [HttpGet]
   [Route("accept/{id}/")]
   public InsuranceClaim Accept(int id)
   {
  	   var insuranceClaim = loadClaim(id);
	   insuranceClaim.Accept();
	   return insuranceClaim;
   }

   [HttpGet]
   [Route("createClaim/{owner}/{amount}")]
   public InsuranceClaim CreateClaim(string owner, int amount)
   {
   	   return new InsuranceClaim(owner, amount);
   }
   
   [Route("claim/{id}")]
   public InsuranceClaim GetClaim(int id)
   {
		return loadClaim(id);
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