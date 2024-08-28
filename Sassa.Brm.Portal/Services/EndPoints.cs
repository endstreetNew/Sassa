namespace Sassa.Brm.Portal.Services
{
    public class EndPoints : Dictionary<string, string>
    {
        public EndPoints(IConfiguration config)
        {
            Add("ProdOneBrm", config[@"EndPoints:ProdOneBrm"]!);
            Add("ProdTwoBrm", config[@"EndPoints:ProdTwoBrm"]!);
            Add("QaBrm", config[@"EndPoints:QABrm"]!);
            Add("Reports", config[@"EndPoints:Reports"]!);
            Add("Kofax", config[@"EndPoints:Kofax"]!);
            Add("TheDocumentWarehouse", config[@"EndPoints:TheDocumentWarehouse"]!);
        }
    }
}
