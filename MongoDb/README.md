# MongoDb

## Task

Sumodeliuokite raktas-reikšmė duomenų bazę bei parašykite ją naudojančią programą įgyvendindami šiuos reikalavimus:
1. Egzistuoja bent viena esybė su sudėtiniu raktu (pavyzdžiui: viešbučio kambariai pagal lokaciją ir numerį, žmonės pagal vardą ir pavardę, pastatai pagal adresą - miestą, gatvę, namo numerį). Toks raktas neturi būti prieštaringas, t.y. komponentai turi būti aiškiai išskirti ir vienų komponentų reikšmės neturi painiotis su kitų. Pavyzdžiui asmuo Jonas Vakaris Jonaitis, kurio vardas Jonas Vakaris, o pavardė Jonaitis, turi neturėti rakto kolizijos su asmeniu Jonas Vakaris Jonaitis, kurio vardas Jonas, o pavardė Vakaris Jonaitis.
2. Programoje turi egzistuoti viena ar daugiau transakcijų, jų įgyvendinimui panaudokite Redis atomines operacijas, MULTI transakcijas. Atsiskaitant reikia parodyti, kad panaudotos priemonės būtinos užtikrinti duomenų teisingumą.  Pavyzdžiui: (1) duomenų bazė laiko sąskaitas, leidžia pervesti pinigus iš vienos į kitą, nenueinant į minusą ir nepadarant klaidos; (2) viešbučių kambarių rezervavimo sistema užtikrina, kad du keliautojai nerezervuos to paties kambario; (3) bilietų rezervacijos sistema su nurodytais skirtingų klasių bilietais užtikrina, kad kiekvienoje klasėje nebus parduota daugiau bilietų, nei nurodytos ribos.

(Hint: https://redis.io/docs/manual/transactions/)
Programą įkelti abiems autoriams prieš atsiskaitant. Atsiskaitant reikės pakomentuoti programą ir veikimo mechanizmus, bei Redis pagrindus. Būkite pasiruošę!

## Software

* Install [.net core 6.0](https://dotnet.microsoft.com/en-us/download)
* Install [Redis on docker](https://redis.io/docs/stack/get-started/install/docker/)

## Build and run

* dotnet build
* dotnet run
* 

