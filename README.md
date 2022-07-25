# Приложение для управления личными встречами

Приложение позволяет создавать новые встречи, редактировать и удалять существующие, создавать напоминания. Так же есть возможность просмотра расписания за выбранный день с дальнейшим экспортом в файл.

Список доступных команд:
* help     - Открыть список доступных команд
* add      - Добавить встречу
* edit     - Изменить встречу
* del      - Удалить встречу
* find     - Посмотреть расписание встреч за выбранный день
* export   - Экспортировать расписание встреч за выбранный день
* exit     - Выйти из приложения

## Тестирование

### Открыть список доступных команд

Кейс 1
* Ввести команду help
* Ожидаемый результат: в консоли появится информация о доступных командах


### Добавление встречи

Кейс 1
* Ввести команду add
* Создать встречу на завтра, указать дату и время начала и окончания, добавить расписание. Напоминание не создавать
* Сохранить встречу
* Ожидаемый результат: Встреча будет создана без ошибок, если использовать команду find и указать завтрашнюю дату, то отобразится только что созданное событие

Кейс 2

* Выполнить Кейс 1, но добавить напоминание. 
* Если сейчас 10:30, то время для события нужно указать 10:35 и поставить напоминание "24:00"
* Дождаться, когда наступит время 10:35
* Ожидаемый результат: при наступлении времени 10:35 в консоли появится строка с кведомлением

Кейс 3

* Создать событие на вчерашний день
* Ожидаемый результат: Не удалось сохранить событие, ошибка

Кейс 4

* Создать событие на завтра, например, с 10:00 до 11:00. Сохранить
* СОздать еще одно событие на завтра, которое пересекается с первым. Например, с 10:30 до 11:30. Сохранить
* Ожидаемый результат: Не удалось сохранить второе событие, ошибка о наложении

Кейс 5

* Создать событие на завтра, но поменять местами дату начала и дату окончания. Сохранить
* Ожидаемый результат: Не удалось сохранить событие, ошибка

Кейс 6

* При создании события задать невалидную строку и(или) поменять местами все параметры
* Ожидаемый результат: Не удалось содать событие, ошибка

Кейс 7

* Создать событие на завтра с корректными данными
* На этапе подтверждения операции выбрать "нет"
* Ожидаемый результат: будет отменено создание события

### Изменить встречу

Кейс 1

* Создать событие на завтра и сохранить
* Определить идентификатор только что созданного события (можно командой find)
* Вызвать команду edit и передать идентификатор
* Внести изменения в событие согласно кейсам Добавления встречи
* Ожидаемый результат: В случае успешного сохранения, будет перезаписанно ранее созданное событие на новые данные

Кейс 2

* Повторить Кейс 1, при создании и при редактировании добавить разные напоминания. Сохранить
* Ожидаемый результат: Отработает только новое напоминание, старое будет удалено

Кейс 3

* Передать в команду edit не существующий идентификатор события, 0 и отрицательное число
* Ожидаемый результат: ошибка о неуспешной попытке найти событие

Кейс 4

* Выполнить кейс 1, но в случае подтверждения сохранения ввести "нет"
* Ожидаемый результат: будет отменено сохранение события, метод find покажет старые данные

### Удалить встречу

Кейс 1

* Создать событие на завтра и сохранить
* Определить идентификатор только что созданного события (можно командой find)
* Вызвать команду del и передать идентификатор
* Ожидаемый результат: Событие удалено

Кейс 2

* Передать в команду del не существующий идентификатор события, 0 и отрицательное число
* Ожидаемый результат: ошибка о неуспешной попытке найти событие

Кейс 3

* Выполнить кейс 1, но в случае подтверждения ввести "нет"
* Ожидаемый результат: будет отменено удаление события

### Посмотреть расписание встреч за выбранный день

Кейс 1

* Создать несколько событий на определенную дату, сохранить
* Вызвать команду find и передать дату
* Ожидаемый результат: В консоли отобразятся все события, созданные на эту дату

Кейс 2

* Вызвать команду find и передать дату на которую точно встречи не создавались
* Ожидаемый результат: В консоли отобразится информация, что событий на выбранную дату нет

Кейс 3

* Вызвать команду find и передать невалидное значение даты
* Ожидаемый результат: Ошибка

### Экспортировать расписание встреч за выбранный день

Кейс 1

* Создать несколько событий на определенную дату, сохранить
* Вызвать команду export и передать дату
* Ожидаемый результат: Экспорт пройдет успешно, в консоли появится полный путь до файла с содержимым в json-формате

Кейс 2

* Вызвать команду export и передать дату на которую точно встречи не создавались
* Ожидаемый результат: В консоли появиится информация, что события не найдены, файл создан не будет

Кейс 3

* Вызвать команду export и передать невалидное значение даты
* Ожидаемый результат: Ошибка

### Выйти из приложения

Кейс 1
* Ввести команду exit
* Ожидаемый результат: приложение завершит работу

### Выйти из приложения

Кейс 1
* Ввести несуществующую команду
* Ожидаемый результат: появится ошибка в консоли, что команда не найдена