# JWT Token Issuer Sample Application

This folder contains a [JSON Web Token](https://datatracker.ietf.org/doc/html/rfc7519) generation application to use with Video Analyzer.

## Prerequisites 
  * Install [.Net Core 3.1 SDK](https://dotnet.microsoft.com/download)

## Build the JWTTokenIssuer
1.  Download the files
2.  Open a CMD prompt and navigate to the folder you downloaded the files to
3.  Run `dotnet build`
4.  Run `dotnet run`
   
**NOTE:** The dotnet build created bin folder in the same directory.  Path to the compiled application is -  {directory}\bin\Debug\netcoreapp3.1\JwtTokenIssuer.exe

## Usage

General usage:

```
 JwtTokenIssuer [--audience=<audience>] [--issuer=<issuer>] [--expiration=<expiration>] [--certificatePath=<filepath> --certificatePassword=<password>]
 ```

Generating from a one time self-signed certificate:

  ```JwtTokenIssuer
  JwtTokenIssuer
  ```

[!NOTE] This will auto-generate a self-signed certificate. **It is highly recommended that you do not use an auto-generated certificate**.  If you continue to use the auto-generated certificate you will be required to update the Video Analyzer's access policy every time you generate a new token to reflect the new certificate's Issuer, Audience, Key Type, Algorithm, Key ID, RSA Key Module, and the RSA Key Exponent.

Generating from a one time self-signed certificate with custom parameters:

  ```JwtTokenIssuer
  JwtTokenIssuer --audience=https://videoanalyzer.azure.net/videos/myvideo --issuer=https://contoso.com --    expiration=2120  -01-01T00:00:00.000Z
  ```

Generating from an existing RSA certificate:

```JwtTokenIssuer
JwtTokenIssuer --audience=https://videoanalyzer.azure.net/videos/myvideo --issuer=https://contoso.com --expiration=2120-01-01T00:00:00.000Z --certificatePath=C:temp\certificate.p12 --certificatePassword=myPassword
```

## Output

This application outputs both the key information needed for the access policy and the token which is used to playback the video.

## Create a Self-Signed Certificate using OpenSSL

Using OpenSSL we can generate certificates.  This allows for one certificate to generate different tokens that can be used for video playback.

1.  Create the certificate:

	```bash
	openssl req -newkey rsa:2048 -nodes -keyout key.pem -x509 -days 365 -subj "/CN=contoso.com" -out certificate.pem
	```

2.  Export to PKCS12:

	```bash
	openssl pkcs12 -inkey key.pem -in certificate.pem -export -out certificate.p12
	```

This certificate can now be used to generate JWT tokens.
