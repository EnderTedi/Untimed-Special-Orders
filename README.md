![untimed-special-orders](https://i.imgur.com/OOwROeR.png)
# Untimed Special Orders!


Untimed Special Orders is a framework that allows modders to surpass the vanilla One Month maxmimun limitation of Special Orders, allowing them to create Special Orders with no time limit.
[Nexus](https://www.nexusmods.com/stardewvalley/mods/26117)



## Usage

### For Players
This mod does nothing by itself and requires another mod to use its function. Install if a mod you've downloaded requires it.

### For Modders
 To remove the time limit off a Special Order add `"Untimed": "true"` in your Special Order's CustomFields.

`Untimed` is **not** a bool and the value can be anything, your Special Order will be untimed as long as `Untimed` is present in the CustomFields.
#### Example:
```json
"<Your Special Order ID>": {
    "Name": "...",
    ...
    <Other Data>
    ...
    "CustomFields": {
        "Untimed": "true"
    }
}
```
