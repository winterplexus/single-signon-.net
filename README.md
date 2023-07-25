Single Sign-on .NET
===================

Single sign-on is an applications that signs-on to a target site without requiring users to enter their password. The applications rely on Azure Active Directory, certificates and SAML2 protocol support.

Single sign-on is based on .NET Framework platform (4.8) and .NET Core (7.0) using the following components:

* Authenticator
    
* Token Generator

## Authenticator

Authenticator is a web application based on .NET Core platform that performs authentication only.

Authenticator must be registered with Azure Active Directory as an Enterprise Application with a redirect URI. The Enterprise Application must be configured to use SAML2 to give Single Sign-on (SSO) access to any application that uses its login URL.

So, when a user launches Authenticator the Enterprise Application login URL is called and if the user is already signed on to Azure Active Directory, the user is sent to the redirect URI; otherwise, the user must select their username or enter their username, password to continue.

Upon redirection, Authenticator will validate and parse the SAML2 response for username and email assertions and call the generate SAML token RESTful web service to generate a SAML token for the target site.

Authenticator will send an HTML response containing the SAML token as a form field to the user's browser and the user will automatically be redirected to the target site.

## Token Generator

Token Generator is a RESTful web service based on .NET Framework platform that performs SAML token generation only.

Token Generator returns a SAML token for the target site using username and email Azure Active Directory claims.

### Project Components

| Name           | Technology                                                  |
| ---            | ---                                                         |
| Authenticator  | ASP.NET MVC (Model View Controller) application server      |
| TokenGenerator | ASP.NET MVC (Model View Controller) REST-ful service server |
