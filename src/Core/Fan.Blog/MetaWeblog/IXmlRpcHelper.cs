using Fan.Blog.MetaWeblog.Models;

namespace Fan.Blog.MetaWeblog
{
    /// <summary>
    /// Helps parsing and composing of the xmlrpc request and response data.
    /// </summary>
    public interface IXmlRpcHelper
    {
        /// <summary>
        /// Returns an <see cref="XmlRpcRequest"/> given an xml input string from OLW.
        /// </summary>
        XmlRpcRequest ParseRequest(string xml);
        /// <summary>
        /// Returns an xml string of the response to the xmlrpc client.
        /// </summary>
        /// <param name="requestMethodName"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        string BuildOutput(string requestMethodName, XmlRpcResponse response);
        /// <summary>
        /// Returns an xml string of the exception to the xmlrpc client.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        string BuildFaultOutput(MetaWeblogException ex);
        /// <summary>
        /// Returns an xml string of the fault to the xmlrpc client.
        /// </summary>
        /// <param name="faultCode"></param>
        /// <param name="faultString"></param>
        /// <returns></returns>
        string BuildFaultOutput(int faultCode, string faultString);
    }
}