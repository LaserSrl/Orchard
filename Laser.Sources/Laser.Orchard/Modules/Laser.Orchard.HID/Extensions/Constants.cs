
namespace Laser.Orchard.HID.Extensions {
    public static class Constants {
        public const string LocalArea = "Laser.Orchard.HID";
        public const string CacheTokenTypeKeyFormat = @"{0}_HID_TokenType";
        public const string CacheAccessTokenKeyFormat = @"{0}_HID_AccessToken";
    }

    public static class HIDAPIEndpoints {

        public const string DefaultContentType = @"application/vnd.assaabloy.ma.credential-management-1.0+json";

        public const string BaseURIProd = @"https://ma.api.assaabloy.com";
        public const string BaseURITest = @"https://test-ma.api.assaabloy.com";
        public const string BaseURIFormat = @"{0}/credential-management";
        //BaseURI = String.Format(BaseURIFormat, [BaseURIProd|BaseURITest]);
        public const string CustomerURIFormat = @"{0}/customer/{1}";
        //BaseEndpoint = String.Format(CustomerURIFormat, BaseURI);

        // Calls we need to the HID API:
        // - Users: {BaseURI}{CustomerURI}/users
        public const string UsersEndpointFormat = @"{0}/users";
        //UsersEndpoint = String.Format(UsersEndpointFormat, BaseEndpoint);
        //  - Create User: POST to the UsersEndpoint
        //  - Search Users: {UsersEndpoint}/.search //we need to first search (to get the userId given its externalID), and then get the user
        public const string UserSearchEndpointFormat = @"{0}/.search"; //search is done in POST
        //UsersSearchEndpoint = String.Format(UserSearchEndpointFormat, UsersEndpoint);
        //  - Create Invitation: {UsersEndpoint}/{userID}/invitation
        public const string CreateInvitationEndpointFormat = @"{0}/{1}/invitation";
        //CreateInvitationEndpoint = String.Format(CreateInvitationEndpointFormat, UsersEndpoint, userId);
        //  - Get Specific User: {UsersEndpoint}/{userID}
        //However, the search returns directly the correct "path" for a user
        //  - Issue Credentials: POST to {BaseEndpoint}/credential-container/{credContainerID}/credential
        public const string IssueCredentialEndpointFormat = @"{0}/credential-container/{1}/credential";
        //IssueCredentialEndpoint = String.Format(IssueCredentialEndpointFormat, BaseEndpoint, credContainerId);
        //we find the id of the container in the json returned from a get user
        //  - Revoke Credentials: DELETE to {BaseEndpoint}/credential/{credentialId}
        public const string RevokeCredentialEndpointFormat = @"{0}/credential/{1}";
        //RevokeCredentialEndpoint = string.Format(RevokeCredentialEndpointFormat, BaseEndpoint, credentialId);

        public const string IdentityProviderProd = @"https://idp.hidglobal.com";
        public const string IdentityProviderTest = @"https://test.idp.hidglobal.com";
        public const string LoginEndpointFormat = @"{0}/idp/SISDOMAIN/authn/token";
        //LoginEndpoint = String.Format(LoginEndpointFormat, [IdentityProviderProd|IdentityProviderTest]);
    }
}