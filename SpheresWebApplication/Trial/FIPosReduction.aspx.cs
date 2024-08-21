using System;
using System.Collections;
using System.Web.UI;

public partial class Trial_FIPosReduction : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ArrayList values = new ArrayList
            {
                new PositionData("Microsoft", "Msft"),
                new PositionData("Intel", "Intc"),
                new PositionData("Dell", "Dell")
            };

            Repeater1.DataSource = values;
            Repeater1.DataBind();
        }
    }
    protected override void OnLoadComplete(EventArgs e)
    {
        base.OnLoadComplete(e);
        _ = Repeater1.Items[0].DataItem;
    }
    protected override void OnPreRender(EventArgs e)
    {
        base.OnPreRender(e);
        _ = Repeater1.Items[0].DataItem;
    }

    protected override void Render(HtmlTextWriter writer)
    {
        base.Render(writer);
        _ = Repeater1.Items[0].DataItem;
    }

}


public class PositionData
{

    private readonly string name;
    private readonly string ticker;

    public PositionData(string name, string ticker)
    {
        this.name = name;
        this.ticker = ticker;
    }

    public string Name
    {
        get
        {
            return name;
        }
    }

    public string Ticker
    {
        get
        {
            return ticker;
        }
    }
}