#
#  create-certificates.ps1
#
#  Copyright (c) Wiregrass Code Technology 2021
#

# generate X509 certificate with public and private keys

openssl genrsa -out authenticate-private.pem 4096
openssl req -new -x509 -sha256 -key authenticate-private.pem -out authenticate.pem -days 1095

# convert to cer format

openssl x509 -inform PEM -in authenticate.pem -outform DER -out authenticate.cer

# convert to pfx format

openssl pkcs12 -inkey authenticate-private.pem -in authenticate.pem -export -out authenticate.pfx

# extract the key-pair

openssl pkcs12 -in authenticate.pfx -nocerts -nodes -out key-pair.key

# get the private key from key-pair

openssl rsa -in key-pair.key -out private.key

# get the public key from key-pair

openssl rsa -in key-pair.key -pubout -out public.key
