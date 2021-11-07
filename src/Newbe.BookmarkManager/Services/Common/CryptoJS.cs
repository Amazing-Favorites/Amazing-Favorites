


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


    public ValueTask<string> MD5(byte[] bytes) => InvokeAsync<string>("hex", bytes);
    public ValueTask<string> MD5(string str) => InvokeAsync<string>("hex", str);
}