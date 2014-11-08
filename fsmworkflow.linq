<Query Kind="Program">
  <Reference Relative="stateful\Automata\bin\Debug\Automata.dll">C:\depot_git\workflow\stateful\Automata\bin\Debug\Automata.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Caching.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <NuGetReference>Microsoft.AspNet.WebApi.OwinSelfHost</NuGetReference>
  <Namespace>Automata</Namespace>
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
  <Namespace>System.Runtime.Serialization</Namespace>
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
	cache.Add("cash", new MoneyBin(100000), DateTime.MaxValue);
	var claim = new InsuranceClaim { Owner = "bjartnes", RequestAmount = 10000};
	
	using (var app = WebApp.Start<Startup>(url: baseAddress))
	{
		"Workflow engine started".Dump();
		Console.ReadLine();
	}
}

// Dette er åpenbart FØR vi subklasser... Subklassing gjør det bedre, OO gjort riktig.
// Strukturen er gitt av 
[DataContract]
public class InsuranceClaim {
	[DataMember]
	public readonly int Id;
	public string Owner {get;set;}
	public int ApprovedAmount {get;set;}
	public int? RequestAmount {get;set;}
	public int? PayedAmount {get;set;}
	public string Approver {get; set;}

	public Automaton<string> Machine {get; set;}
	public List<State<string>> InsuranceStates {get; set;}
    // ALWAYS CHECK FOR CORRECT STATE IN METHOD BEFORE PROCESS PAYMENT

	public void Draw() {
	    Util.ClearResults();
		var thing = new DrawThing();
		var str = thing.GetDotGraphString(Machine, InsuranceStates);
		var img = thing.CreateGraphImage(str);
		img.Dump();
		MemoryCache.Default.Get("cash").Dump();
	}


	public InsuranceClaim () {
		var cache = MemoryCache.Default;
		var init = new State<string>("init", () => {"INITSTATE".Dump(); });
		var accepted = new State<string>("accepted", () => { ApprovedAmount = 100; MakePayment();});
		init.To(accepted).On("approve");
		InsuranceStates = new List<State<string>> {init, accepted};
		Machine = new Automaton<string>(InsuranceStates.First());
		Id = cache.Count();
		cache.Add("claim"+Id, this, DateTime.MaxValue);
		Draw();
	}
	
	public InsuranceClaim Create(string owner, int requestAmount) {
		var claim = new InsuranceClaim() { Owner = owner, RequestAmount =requestAmount};
		Draw();
		return claim;
	}
	
	public void AcceptAmount() {
		MakePayment();
		SetPaymentComplete();
	}
	
	public void SetPaymentComplete() {
	}
	
	private void MakePayment() {
	   var moneyBin = (MoneyBin)MemoryCache.Default.Get("cash");
	   moneyBin.Money = moneyBin.Money - ApprovedAmount;
	   PayedAmount = ApprovedAmount;
	}

	public void ProcessComplaint (int? newAmount, string approver) {
		Approver = approver;
		ApprovedAmount = newAmount.Value;
	}

	public void MakeComplaint() {
	}
	
	public void SetAcceptedAmount(int amount, string approver, string password) {
		if (password != "admin") return;
		ApprovedAmount = amount;
		Approver = approver;
	}
}

public class InsuranceClaimController : ApiController
{
	private InsuranceClaim _insuranceClaim;
	private InsuranceClaim loadClaim(int id){
	   return MemoryCache.Default.Get("claim"+id) as InsuranceClaim;
	}
	public InsuranceClaimController () {
//	 Try to popualate with correct id based on route
	   _insuranceClaim = MemoryCache.Default.Get("claim1") as InsuranceClaim;
	}

   [Route("testaccept/{id}")]
   public InsuranceClaim GetSetAcceptedAmount(int id)
   {
  	   _insuranceClaim = loadClaim(id);
	   _insuranceClaim.Machine.Accept("approve");
	   _insuranceClaim.Draw();
	   return _insuranceClaim;
   }

   [Route("setAcceptedAmount/{id}/{approver}/{amount}/{password}")]
   public InsuranceClaim GetSetAcceptedAmount(int id, string approver, int amount, string password)
   {
  	   _insuranceClaim = loadClaim(id);
	   _insuranceClaim.SetAcceptedAmount(amount, approver, password);
	   return _insuranceClaim;
   }

   [Route("ProcessComplaint/{approver}/{newAmount}")]
   public InsuranceClaim GetProcessComplaint (string approver, int? newAmount)
   {
	   _insuranceClaim.ProcessComplaint(newAmount,approver);
       return _insuranceClaim;
   }

   [Route("sendToEvaluation/{owner}/{amount}")]
   public InsuranceClaim GetSendToEvalution(string owner, int amount)
   {
   	   _insuranceClaim.Create(owner, amount);
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