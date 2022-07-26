# Futurum.OpenApi to PlantUml

![license](https://img.shields.io/github/license/futurum-dev/futurum.openapi-to-plantuml?style=for-the-badge)

Creates [PlantUml](https://plantuml.com) diagrams from [OpenApi](https://www.openapis.org) spec files.

## Example 1
[OpenApi spec](./openapi/openapi-petstore.json) taken from [here](https://petstore.swagger.io)

### OpenApi diagram
![OpenApi diagram](./docs/openapi-petstore-openapi.svg)

### OpenApi Type diagram
![OpenApi Type diagram](./docs/openapi-petstore-openapi-type.svg)

## Example 2
[OpenApi spec](./openapi/openapi-petstore-simple.yaml)

### OpenApi diagram
![OpenApi diagram](./docs/openapi-petstore-simple-openapi.svg)

### OpenApi Type diagram
![OpenApi Type diagram](./docs/openapi-petstore-simple-openapi-type.svg)

## How to use it
### Console
Use *futurum.openapi-to-plantuml-console* project

#### File
Use *file* followed by file path
```
file --path "../../../../openapi/openapi-petstore.json"
```

#### Directory
Use *directory* followed by directory path
```
directory --path "../../../../openapi"
```

#### Diagram Configuration
#### Theme
Use *--theme* to specify the PlantUml theme
```
--theme "blueprint"
```
*NOTE - defaults to no theme*

#### Show notes
Use *--shownotes* to specify if PlantUml should show notes
```
--shownotes "true"
```
*NOTE - defaults to no notes*

### Docker - Use with multiple OpenApi spec files

```
docker run --rm -it -v $(pwd)/openapi:/openapi futurum.openapi-to-plantuml-directory
```

#### Diagram Configuration
#### Theme
Use *--theme* to specify the PlantUml theme
```
--theme "blueprint"
```
*NOTE - defaults to no theme*

e.g.
```
docker run --rm -it -v $(pwd)/openapi:/openapi futurum.openapi-to-plantuml-directory --theme "blueprint"
```

#### Show notes
Use *--shownotes* to specify if PlantUml should show notes
```
--shownotes "true"
```
*NOTE - defaults to no notes*

e.g.
```
docker run --rm -it -v $(pwd)/openapi:/openapi futurum.openapi-to-plantuml-directory --shownotes "true"
```

### Docker - Use with individual OpenApi spec files
#### OpenApi diagram
```
cat ./openapi/openapi-petstore.json | docker run --rm -a stdin -a stdout -i futurum.openapi-to-plantuml-std-in-out openapi |> ./openapi/openapi-petstore-openapi.puml
```
```
cat ./openapi/openapi-petstore-simple.yaml | docker run --rm -a stdin -a stdout -i futurum.openapi-to-plantuml-std-in-out openapi |> ./openapi/openapi-petstore-simple-openapi.puml
```

#### OpenApi Type diagram
```
cat ./openapi/openapi-petstore.json | docker run --rm -a stdin -a stdout -i futurum.openapi-to-plantuml-std-in-out openapi-type |> ./openapi/openapi-petstore-openapi-type.puml
```
```
cat ./openapi/openapi-petstore-simple.yaml | docker run --rm -a stdin -a stdout -i futurum.openapi-to-plantuml-std-in-out openapi-type |> ./openapi/openapi-petstore-simple-openapi-type.puml
```

#### Diagram Configuration
#### Theme
Use *--theme* to specify the PlantUml theme
```
--theme "blueprint"
```
*NOTE - defaults to no theme*

e.g.
```
cat ./openapi/openapi-petstore.json | docker run --rm -a stdin -a stdout -i futurum.openapi-to-plantuml-std-in-out openapi --theme "blueprint" |> ./openapi/openapi-petstore-openapi.puml
```

#### Show notes
Use *--shownotes* to specify if PlantUml should show notes
```
--shownotes "true"
```
*NOTE - defaults to no notes*

e.g.
```
cat ./openapi/openapi-petstore.json | docker run --rm -a stdin -a stdout -i futurum.openapi-to-plantuml-std-in-out openapi --shownotes "true" |> ./openapi/openapi-petstore-openapi.puml
```

## Docker
### How to build it

```
docker build -t futurum.openapi-to-plantuml-directory -f futurum.openapi-to-plantuml-directory/Dockerfile .
```
```
docker build -t futurum.openapi-to-plantuml-std-in-out -f futurum.openapi-to-plantuml-std-in-out/Dockerfile .
```

### Remove images
```
docker rmi futurum.openapi-to-plantuml-directory
```
```
docker rmi futurum.openapi-to-plantuml-std-in-out
```