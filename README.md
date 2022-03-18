# ✨Irehon

#### This is a repository with source code of Irehon server and client both

### [🎮 Irehon on steam](https://store.steampowered.com/app/1759510)

---
- [Game description](#game-description)
- [Project status](#project-status)
- [Credits](#credits)
- [Installation on dedicated server](#installation-on-dedicated-server)
    - [Docker image](#docker-image)
    - [Run multiple docker images of server](#run-multiple-docker-images-of-server)
- [Install client](#install-client)

## 📙Game description

Irehon — non-target Action-MMORPG, its story is set in fantasy world with two opposing factions. Player has to explore the large open world, full of dangerous places, enemies, bosses, and other inhabitants.

## 📝Project status
Development started in January 2021 and due to sanctions we can no longer continue development

We have decided to publish the source code of our work

> You may not use these sources for commercial purposes, otherwise use them as you wish.

##  🤝Credits

- [👤 **Niyaz Kashafutdinov**](https://github.com/re-mouse)

- [👤 **Davlet Sibgatullin**](https://github.com/dpatrica)

- [👤 **Aidar Farutdinov**](https://github.com/aidarf)

- [👤 **Vitalii Evstratov**](https://github.com/vesord)

- [👤 **Alexandr Pikhenko**](https://github.com/sjacki)


## 🚀Installation on dedicated server

To run server you should install:
- Database
- RestAPI server, you can use [our api code](https://github.com/re-mouse/Irehon-php-api), or crea your own, that implement minimal functionality
- Builds storage

Clone repository

```bash
git clone https://github.com/re-mouse/Irehon.git
```

After you installed API server, you should change url in Assets/Scripts/Api/Api.cs
```csharp

public static UnityWebRequest Request(string request, ApiMethod method = ApiMethod.GET, string body = null)
{
    string uri = "https://testapi.irehon.com" + request; //Change it to your url
    UnityWebRequest www;
    if (body != null && body != "")
    {
        www = UnityWebRequest.Put(uri, body);
        www.method = method.ToString();
    }
    else
        www = new UnityWebRequest(uri);
    www.downloadHandler = new DownloadHandlerBuffer();
    www.method = method.ToString();
    www.SetRequestHeader("Cookie", API_KEY_COOKIE);
    Debug.Log($"Request created: {www.uri} {www.method}");

    return www;
}
```

`Build` on platform that you need

> - Master branch contain server code
>
> - Client branch contains client code

Upload build on your build storage, to make it available via wget

---
### Docker image
In this example we use uploaded rar file, that contains our build on remote build storage
> Replace "location" with location that you running
```dockerfile
FROM ubuntu:latest
RUN apt-get update
RUN apt-get install -y wget unrar lib32gcc1
RUN cd /

#to use steamworks sdk
RUN wget https://steamcdn-a.akamaihd.net/client/installer/steamcmd_linux.tar.gz
RUN mkdir /steamcmd
RUN tar -xvzf steamcmd_linux.tar.gz -C /steamcmd
RUN /steamcmd/linux32/steamcmd; exit 0
RUN mkdir -p /root/.steam/sdk64/
RUN mv /steamcmd/linux64/steamclient.so /root/.steam/sdk64/

RUN wget http://remote.buildstorage.com/Location.rar #replace this url to your build storage

RUN mv Location.rar /root/
RUN unrar x -y -r /root/Location.rar
RUN chmod -R a+xwr /Location/*

CMD ["/Location/Irehon"]
```

---

### Run multiple docker images of server
With this script you can run multiple server instances.
> For 1 instance requirments:
> - 1.7GB RAM - Max usage with 30 players on instance
> - 2 core - 1 instance - To avoid lags with 30 players on instance
> - Storage memory required only to store docker images. ~1.7GB per image

```bash
PORT=41000 #Replace it with port, that you running

north=3 #type required location with it required quantity
center=2
south=3

locations=("north" "south" "center")

for location in ${locations[@]}; do
    eval "count=\$$location"
    if [[ "$count" != "$empty" ]]; then
        docker build . --no-cache --tag $location -f ${location^}Dockerfile
        for i in $(seq 1 $count);
            do
            docker run -d -p $PORT:$PORT/udp -p $PORT:$PORT/tcp --env SERVERPORT=$PORT $location
            echo "Runned $i $location server, port = $PORT"
            PORT=$(($PORT + 50))
        done
    fi

done
```

## 🤖Install client
Clone repository

```bash
git clone https://github.com/re-mouse/Irehon.git
```
Switch to `client` branch
```bash
git checkout client
```
Build on platform that you need
