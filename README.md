# TestMT
Use different MT engines and display differences, with an optional reference and an optional human translation.

This sample project aims at showing differences between some of the main commercial MT engines available online.
In particular we are using Microsoft, Google, ModernMT.
Using different MTs can increase not only productivity but creativity of professional translators, as well as stimulate a debate on the capabilities and shortcomings of current MT engines.

This app has been first presented at the workshop "MACHINE TRANSLATION AND HUMAN CREATIVITY" in Bologna, Italy, in 2021.
https://www.bolognachildrensbookfair.com/eventi/open-up/edizione-2021/machine-translation-and-human-creativity/10615.html

The project uses MVC on .Net Core 3.1.

**Important notes about Keys to MT APIs**
For Google, you will need a settings file like unibo-literary-translation-09aff375131b.json which is mentioned in the code (Translator.cs) but not provided in this repository.
For other engines, the keys are defined in the appsettings.json file, and defined in the Web App settings, on the Azure Portal site.


