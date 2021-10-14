


using System.Linq;
using System.Threading.Tasks;
using JsBind.Net;

public class CryptoJS : ObjectBindingBase
{
    public CryptoJS(IJsRuntimeAdapter jsRuntime)
    {
        SetAccessPath("md5");
        Initialize(jsRuntime);
    }


    public ValueTask<string> Hex(byte[] bytes) => InvokeAsync<string>("hex", string.Join("", bytes.Select(x => x.ToString("X"))));
    public ValueTask<string> Hex(string str) => InvokeAsync<string>("hex", str);
}