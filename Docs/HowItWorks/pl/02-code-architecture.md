# 02 · Architektura kodu

**W jednym zdaniu:** reguły gry działają jako zwykły kod C# (symulacja), całkowicie oddzielony od sceny Unity, która tylko wyświetla grę i przekazuje symulacji polecenia gracza.

**Stan:** foldery modułów i ich pliki definicji assembly istnieją. Pętla symulacji (zegar, kalendarz, szyna zdarzeń; zob. 03) i siatka budowania (zob. 05) są gotowe. Scena nie uruchamia jeszcze symulacji, a pozostałe moduły są na razie puste; fragmenty poniżej, które ich dotyczą, opisują projekt w trakcie budowy.

## Po co nam to

Ekonomiczna gra strategiczna to głównie reguły i liczby: surowce, energia, produkcja, koloniści, dni. Gdyby te reguły były rozsiane po obiektach sceny (skryptach `MonoBehaviour`), zależałyby od tego, co jest wczytane w scenie, od liczby klatek na sekundę i od trybu Play. Trudno byłoby je testować i zapisywać, a łatwo przypadkiem zepsuć.

## Jak to działa

1. **Dwie warstwy.**
   - **Symulacja** przechowuje stan gry (co jest w magazynie, jakie budynki istnieją, który jest dzień) i wszystkie reguły. To zwykły C#: bez `MonoBehaviour`, bez `Time.deltaTime`, bez obiektów sceny.
   - **Warstwa prezentacji** (scena, modele, interfejs) odczytuje stan symulacji, żeby go narysować, i zamienia kliknięcia gracza na polecenia dla symulacji („postaw tutaj silos”).
2. **Jeden punkt wejścia.** `GameBootstrap`, pojedynczy komponent w scenie, tworzy symulację po wczytaniu sceny i raz na klatkę wywołuje `Simulation.Update`.
3. **Moduły.** Każdy system gry to moduł we własnym folderze w `Assets/_Project/Scripts/` (Economy, Population, Building, World, Terraform, Combat, Expedition, Tutorial, Persistence, UI, Game).
4. **Assembly.** Każdy moduł ma plik definicji assembly (`.asmdef`). Dzięki niemu Unity kompiluje moduł osobno, a sam plik wymienia moduły, z których wolno mu korzystać. Na przykład `Colony.Economy` może korzystać z `Colony.Core` i `Colony.Data`, ale nie z `Colony.Combat`. Zabroniona zależność po prostu się nie skompiluje.
5. **Zdarzenia między modułami.** Gdy moduły mają na siebie reagować, ale nie mogą od siebie zależeć, jeden publikuje zdarzenie (np. „aktywowano kopułę”), a pozostałe je subskrybują (zob. 03).
6. **Liczby w danych, nie w kodzie.** Koszty, tempo produkcji, pojemności i czasy trwania są zapisane w zasobach ScriptableObject w `Assets/_Project/Data/`, więc projektanci mogą balansować grę w Inspectorze bez dotykania kodu.

## Dlaczego takie rozwiązanie

| Możliwość | Dlaczego nie (albo dlaczego tak) |
| --- | --- |
| Wszystko w skryptach `MonoBehaviour` | Łatwo zacząć, ale reguły zależą od sceny, działają w tempie klatek i da się je testować tylko w trybie Play. Zapis gry wymaga zbierania stanu z wielu obiektów. |
| Jedno duże assembly (domyślne w Unity) | Każdy skrypt może użyć każdego innego, więc zależności z czasem się plączą, a każda zmiana kompiluje wszystko od nowa. |
| Unity DOTS / ECS | Stworzone dla ogromnej liczby obiektów; nasza kolonia ma ich najwyżej kilkaset. Dodałoby złożoność i czas nauki bez korzyści, której potrzebujemy. |
| **Symulacja w zwykłym C# + cienka warstwa sceny (wybrane)** | Reguły testuje się w milisekundach, zapis gry to zapisanie jednego zestawu obiektów, wyniki nie zależą od liczby klatek na sekundę, a moduły pozostają niezależne. |

## Gdzie to jest w projekcie

- `Assets/_Project/Scripts/<Module>/Colony.<Module>.asmdef`: granice modułów i dozwolone odwołania.
- `Assets/_Project/Scripts/Game/`: zawiera `GameBootstrap`, czyli połączenie sceny z regułami gry. Obecnie tworzy on siatkę budowania i elementy odpowiedzialne za stawianie budynków (zob. 05); tworzenie i aktualizowanie symulacji zostanie dodane w zadaniu ze sceną główną.
- `Docs/Architecture.md`: pełna tabela modułów i konwencje pisania kodu.

## Skąd wiemy, że działa

- Projekt się kompiluje: zabronione odwołanie między modułami byłoby błędem kompilacji.
- Każda reguła symulacji ma testy Edit Mode w `Assets/_Project/Tests/EditMode/`, które działają bez żadnej sceny. Na razie: rdzeń symulacji (`Core/`, zob. 03) i siatka budowania (`Building/`, zob. 05).

## Pytania, które mogą paść

**Dlaczego nie użyć po prostu `Update()` w każdym budynku?** Każdy budynek działałby w tempie klatek, kolejność aktualizacji byłaby nieprzewidywalna, a reguł nie dałoby się testować ani zapisywać w jednym miejscu. Jedna symulacja, która wykonuje tick każdego systemu w stałej kolejności, usuwa wszystkie trzy problemy.

**Czym jest definicja assembly?** To plik, który mówi Unity, żeby skompilowało folder jako osobną bibliotekę, i wskazuje, z jakich innych bibliotek wolno jej korzystać. Pilnuje granic naszych modułów i przyspiesza ponowną kompilację.

**Skąd interfejs wie, co pokazać?** Odczytuje stan symulacji i subskrybuje jej zdarzenia. Nigdy nie zmienia stanu bezpośrednio, tylko wysyła polecenia.

**Gdzie projektant zmieni koszt budynku?** W zasobie ScriptableObject tego budynku w `Assets/_Project/Data/`, w Inspectorze. Bez zmiany kodu.

**Dlaczego kod trzyma się tak ścisłych zasad nazewnictwa i stylu?** Kod czyta się znacznie częściej, niż się go pisze, a cztery osoby muszą umieć go wyjaśnić. Dzięki stałym zasadom (wypisanym w `Docs/Architecture.md` > Conventions) każdy plik wygląda tak samo, więc sama nazwa mówi, czym coś jest: wielkie litery oznaczają stałą (`MAX_FRAME_SECONDS`), podkreślenie na początku oznacza pole prywatne (`_elapsedSeconds`), a `On` na początku oznacza zdarzenie (`OnDayStarted`).

**Dlaczego nie ma obiektów globalnych („singletonów”), do których może sięgnąć każdy skrypt?** Unity zachowuje wartości globalne (statyczne) między kolejnymi uruchomieniami testów w edytorze, więc stary stan mógłby przedostać się do nowej gry. Zamiast tego jeden obiekt sceny, `GameBootstrap`, będzie tworzył symulację i przekazywał ją wszystkiemu, co jej potrzebuje.
