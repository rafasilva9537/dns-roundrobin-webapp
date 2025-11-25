Here is the content formatted into clean, structured Markdown.

-----

# dns-roundrobin-webapp

Multi-server ASP.NET web application load-balanced via DNS Round Robin, where users authenticate using JWT and remain logged in across all backend nodes thanks to centralized session persistence.

This project demonstrates horizontal scaling, DNS-based load balancing, and state sharing across instances in a fully containerized environment.

## DNS Setup

The following steps describe how to configure the development machine (Debian/Ubuntu/Mint) so the BIND9 DNS container and the backend API containers can be reached from any device in the LAN.

The key strategy is **IP aliasing**: assigning extra LAN IPs directly to the host interface and mapping each backend container to one of these IPs.

### 1\. Disable systemd-resolved DNS stub (to free port 53)

Debian-based distros run `systemd-resolved`, which binds to port 53 on localhost. This conflicts with the BIND9 container that also needs to listen on port 53.

Edit the config:

```bash
sudo nano /etc/systemd/resolved.conf
```

Set:

```ini
DNSStubListener=no
```

(Optional) If you want Docker/BIND9 as system DNS:

```ini
DNS=127.0.0.1
```

Restart service:

```bash
sudo systemctl restart systemd-resolved
```

-----

### 2\. Add available LAN IPs to the host interface (IP aliasing)

This project requires each backend API instance to be reachable via its own IP address in the LAN. These IPs must belong to your local subnet.

#### 2.1 Identify your main interface and subnet

Run:

```bash
ifconfig
```

Example Output:

> `wlp8s0: inet 10.94.217.143  netmask 255.255.255.0`

This means your LAN is: `10.94.217.0/24`

#### 2.2 Scan the network to see which IPs are already in use

```bash
sudo arp-scan --localnet
```

#### 2.3 Choose 3 unused IPs

Example:

  * `10.94.217.151`
  * `10.94.217.152`
  * `10.94.217.153`

#### 2.4 Add these IP aliases to the host network interface

```bash
sudo ip addr add 10.94.217.151/24 dev wlp8s0
sudo ip addr add 10.94.217.152/24 dev wlp8s0
sudo ip addr add 10.94.217.153/24 dev wlp8s0
```

They become fully routable inside your LAN and reachable by your phone, laptop, or any other device.

-----

### 3\. Bind backend containers to those IPs

In `backend/compose.yaml`, map each API container to its dedicated LAN IP.

**Example:**

**API 1**

```yaml
ports:
  - "10.94.217.151:80:8080"
```

**API 2**

```yaml
ports:
  - "10.94.217.152:80:8080"
```

**API 3**

```yaml
ports:
  - "10.94.217.153:80:8080"
```

Now each container has a “real” LAN IP endpoint:

  * `http://10.94.217.151`
  * `http://10.94.217.152`
  * `http://10.94.217.153`

And all are externally visible from your phone.

-----

### 4\. Configure the BIND9 DNS for Round Robin

In the zone file for `meutrabalho.com.br`:

```dns
$TTL 3
$ORIGIN meutrabalho.com.br.

@   IN SOA  ns1.meutrabalho.com.br. admin.meutrabalho.com.br. (
        2025112500 ; serial
        3600       ; refresh
        900        ; retry
        604800     ; expire
        1          ; minimum / negative caching
)

    IN NS   ns1.meutrabalho.com.br.
ns1 IN A    10.94.217.143   ; IP of your DNS host

www IN A    10.94.217.151
www IN A    10.94.217.152
www IN A    10.94.217.153
```

> **Note:** The very low TTL forces clients to request DNS answers frequently, enabling Round Robin switching.

-----

### 5\. Configure your client devices

On any device that should access the app:

Set the DNS to your host machine (running the bind9 container):

  * **DNS:** `10.94.217.143`

Now the domain:

  * `www.meutrabalho.com.br`

will resolve to:

  * `10.94.217.151`
  * `10.94.217.152`
  * `10.94.217.153`

...in a round-robin cycle.

-----

## API Setup
### 1\. Configure User Secrets
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433; Database=UserManagementDb; MultipleActiveResultSets=True; User ID=sa;Password='examplePassword1234'; Encrypt=False;",
    "DbTestConnection": "Server=localhost,1433; Database=UserManagementDbTest; MultipleActiveResultSets=True; User ID=sa;Password='examplePassword1234'; Encrypt=False;"
  },
  "JwtConfig": {
    "Secret": "a-string-secret-at-least-256-bits-long",
    "Issuer": "http://localhost:5201/",
    "Audience": "http://localhost:5201/",
    "AccessTokenExpirationMinutes": "15",
    "RefreshTokenExpirationDays": "7"
  }
}
```