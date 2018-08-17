using PX.Data;

namespace PX.Objects.HackathonDemo
{
    public class InvalidRowException : PXException
    {
        public InvalidRowException()
            : base("Invalid Row")
        {
        }
    }
}
