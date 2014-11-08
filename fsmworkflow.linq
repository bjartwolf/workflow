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
	var claim = new InsuranceClaim ("bjartnes", 10000);
	
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
	public int RequestAmount {get;set;}
	public int PayedAmount {get;set;}
	public string Approver {get; set;}
  	[DataMember]
	public string State {
		get {
			return Machine.State.Name;
		}
		set {
			throw new Exception("That's not how state machines work");
		}
	}
	[DataMember]
	public IEnumerable<string> Transitions {
		get {
			return Machine.State.Transitions.Select( t => t.Target.Name);
		}
		set {
			throw new Exception("This is not Erlang");
		}
	}
	public Automaton<string> Machine {get; set;}
	public List<State<string>> InsuranceStates {get; set;}

	public void Draw() {
	    Util.ClearResults();
		var thing = new DrawThing();
		var str = thing.GetDotGraphString(Machine, InsuranceStates);
		var img = thing.CreateGraphImage(str);
		img.Dump();
		MemoryCache.Default.Get("cash").Dump();
	}


	public InsuranceClaim (string owner, int requestAmount) {
		var created = new State<string>("created", () => { });
		var complained = new State<string>("complained", () => { });
		var reviewed = new State<string>("reviewed", () => { });
		var accepted = new State<string>("accepted", () => {MakePayment(); Machine.Accept("setcomplete");});
		var completed = new State<string>("completed", () => { });
		created.To(reviewed).On("reviewed");
		reviewed.To(accepted).On("accepted");
		reviewed.To(complained).On("complained");
		complained.To(reviewed).On("reviewed");
		accepted.To(completed).On("setcomplete");
		InsuranceStates = new List<State<string>> {created, reviewed, complained, accepted, completed};
		Machine = new Automaton<string>(InsuranceStates.First());
		Owner = owner;
		RequestAmount = requestAmount;
		// Add to cache
		var cache = MemoryCache.Default;
		Id = cache.Count();
		cache.Add("claim"+Id, this, DateTime.MaxValue);
		Draw();
	}
	
	public void Accept() {
		Machine.Accept("accepted");
		Draw();
	}
	
	private void MakePayment() {
	   var moneyBin = (MoneyBin)MemoryCache.Default.Get("cash");
	   moneyBin.Money = moneyBin.Money - ApprovedAmount;
	   PayedAmount = ApprovedAmount;
	   Draw();
	   
	}

	public void Approve (int newAmount, string approver) {
		Approver = approver;
		ApprovedAmount = newAmount;
		Machine.Accept("approved");
	    Draw();
	}

	public void MakeComplaint() {
		Machine.Accept("complained");
 	    Draw();
	}
	
	public void ProcessComplaint(int? newAmount, string approver, string password) {
		if (password != "admin") return;
		if (newAmount.HasValue) {
			ApprovedAmount = newAmount.Value;
		}
		Approver = approver;
		Machine.Accept("reviewed");
		Draw();
	}
	
	public void Review(int amount, string approver, string password) {
		if (password != "admin") return;
		ApprovedAmount = amount;
		Approver = approver;
		Machine.Accept("reviewed");
		Draw();
	}
}

public class InsuranceClaimController : ApiController
{
	private InsuranceClaim loadClaim(int id){
	   return MemoryCache.Default.Get("claim"+id) as InsuranceClaim;
	}

   [HttpGet]
   [Route("review/{id}/{approver}/{amount}/{password}")]
   public InsuranceClaim Review(int id, string approver, int amount, string password)
   {
  	   var insuranceClaim = loadClaim(id);
	   insuranceClaim.Dump();
	   insuranceClaim.Review(amount, approver, password);
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
   [Route("ProcessComplaint/{id}/{approver}/{password}/{newAmount}")]
   public InsuranceClaim ProcessComplaint (int id, string approver, string password, int? newAmount)
   {
   	   var insuranceClaim = loadClaim(id);
	   insuranceClaim.ProcessComplaint(newAmount,approver, password);
       return insuranceClaim;
   }

   [HttpGet]
   [Route("createClaim/{owner}/{amount}")]
   public InsuranceClaim CreateClaim(string owner, int amount)
   {
       var insuranceClaim = new InsuranceClaim(owner, amount);
	   insuranceClaim.Draw();
	   return insuranceClaim;
   }

   [HttpGet]
   [Route("")]
   public HttpResponseMessage Result()
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