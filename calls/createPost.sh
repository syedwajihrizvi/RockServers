USERNAME=$1
DATA=$2
PORT=$3
TOKEN=$(grep -o "$USERNAME:[^,]*," ./logs/usernameToToken.txt| cut -d ':' -f2 | tr -d ',')
URL="http://localhost:$PORT/api/posts"
echo "Found Token: $TOKEN"
RESPONSE=$(curl -X POST $URL -H "Content-Type: application/json" -H "Authorization: Bearer $TOKEN" --data "$DATA")
echo "$RESPONSE"
