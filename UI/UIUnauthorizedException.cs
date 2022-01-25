namespace UI
{
    public class UIUnauthorizedException : UIGenericHttpException
    {
        public UIUnauthorizedException() : base(System.Net.HttpStatusCode.Unauthorized)
        {
        }
    }
}