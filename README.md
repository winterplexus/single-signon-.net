Single Sign-on .NET
===================

Single Sign-on is a set of applications based on .NET Core and .NET Framework platforms that signs-on to a target site without requiring users to enter their password. 

The applications perform the following separate actions:

* Authenticate
* Generate

The applications rely on Azure Active Directory, certificates and SAML2 protocol support.

## Authenticate

Authenticator is a web application based on .NET Core platform that performs authentication only.

The Authenticator Enterprise Application must be created in Azure Active Directory. The Authenticator web application must be registered with Azure Active Directory with a redirect URI. The Enterprise Application must be configured to use SAML2 to give Single Sign-on (SSO) access to any application that uses it's login URL.

So, when a user launches Authenticator, the Enterprise Application login URL is called and if the user is already signed on to Azure Active Directory, the user is sent to the redirect URI; otherwise, the user must select their user name or enter their user name, password to continue.

Upon redirection, Authenticator will validate and parse the SAML2 response for user name and email assertions and call the generate SAML token RESTful web service to generate a SAML token for the target site.

Authenticator will send an HTML response containing the SAML token as a form field to the user's browser and the user will automatically be redirected to the target site.

## Generate

* TokenGenerator is a RESTful web service based on .NET Framework platform that performs SAML token generation only.

* TokenGenerator returns a SAML token for the target site using user name and email Azure Active Directory claims.
