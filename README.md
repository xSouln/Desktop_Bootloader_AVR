# Описание программы загрузчика
___
## Содержимое
1. [Установка приложения](#Установка-приложения)
2. [Настройка соединения](#Настройка-соединения)
3. [Загрузка прошивки](#Загрузка-прошивки)

## Установка приложения
- Загрузить актуальную версию загрузчика оп данной [ссылке](https://gitlab.adani.by:2443/rekuts/DivXBootloader-WPF/-/archive/main/DivXBootloader-WPF-main.zip?path=Bootloader_AVR/Application).
- Распоковать архив.
- Файл запуска приложения ../Bootloader_AVR/Application/Bootloader_AVR.exe.

## Настройка соединения
- Открыть приложение Bootloader_AVR.exe.
- Для подключения по SerialPort зайти в меню "ComPort"-"Com port":

![меню сериал порт](/Images/Screenshot_4.png).

- В открывшемся окне выбрать нужную скорость и названеие COM порта, нажать кнопку "Connect":
При успешном подключении подсветка должна сменить фон на зеленый.

![меню сериал порт настройки](/Images/Screenshot_5.png).

- Режиму включенного загрузчика или его наличие соответсвует зеленый фон индикатора "Link".
- Для включения режима загрузчика из основного приложения нажать кнопку "Start boot", при наличии загрузчика будет выван загрузчик и индикатор "Link" станет зеленый. Может потребоваться перезагрузка устройства, сбросом питания, после вызова загрузчика.

## Загрузка прошивки
- Приложение работает с файлами прошивок с раширениями .hex. Для открытия файла прошивки нажать кнопку "File open":

![открытие файла](/Images/Screenshot_7.png).

- В диологовом окне указать путь к файлу. При успешной открытии файла в окне слева будет видно содержимое файла, а в окне справа параметры:

![открытие файла](/Images/Screenshot_6.png).

- При успешном настроеном соединении и успешно открытом файле прошивки можно приступать к загрузке, нажав кнопку "Start load". В ходе загрузки все кнопки будут забликированы до момента окончания загрузки. Ход прошивки можно наблюдать по изминению статусов в окне справа. Для отмены загрузки нажать кнопку "Stop load".
- После успешной загрузке перейти в основное приложение можно нажав кнопку "Start main" - кнопка "Link" при успешном переходе станет красной, или сбросить питание.
