PORT="$1"
DATA="$2"
URL="http://localhost:$PORT/api/accounts/register"
echo "Sending request to following URL:$URL"
RESPONSE=$(curl -s -w  "%{http_code}" -X POST "$URL" -H "Content-Type: application/json" --data "$DATA")
echo "$RESPONSE"
echo "$RESPONSE" >> ./logs/register.txt
