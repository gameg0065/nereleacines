# Cassandra

## Task

Sumodeliuokite nesudėtingą sritį Cassandra duomenų bazėje. Parašykite programą naudojančią duomenų bazę ir leidžiančią atlikti kelias operacijas pasirinktoje srityje.
Su programa pateikite duomenų modelio diagramą.
Savybės sričiai:
1) Egzistuoja bent kelios esybės 2) Yra bent dvi esybės su vienas-su-daug sąryšiu 3) Panaudojimo atvejuose bent vienai esybei reikalingos kelios užklausos pagal skirtingus parametrus
   Pavyzdžiui,  banke saugome klientus, jų sąskaitas (vienas su daug sąryšis) ir kreditines korteles. Sąskaitų norime ieškoti pagal klientą (rasti visas jo sąskaitas) bei pagal sąskaitos numerį, klientų norime ieškoti pagal jų kliento ID arba asmens kodą. Kredito kortelių norime ieškoti pagal jų numerį,  taip pat norime rasti sąskaitą susietą su konkrečia kortele.
   Bent vienoje situacijoje prasmingai panaudokite Cassandra compare-and-set operacijas (hint: IF) INSERT ar UPDATE sakinyje. Pavyzdžiui, norime sukurti naują sąskaitą su kodu tik jei ji neegzistuoja. Norime pervesti pinigus, tik jei likutis pakankamas.
   Užklausose ALLOW FILTERING naudoti negalima!

## Software

* Install [.net core 6.0](https://dotnet.microsoft.com/en-us/download)
* Install [Redis on docker](https://redis.io/docs/stack/get-started/install/docker/)

## Build and run

* dotnet build
* dotnet run
* 

