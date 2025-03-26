while IFS= read -r line; do
        USERNAME=$(echo "$line" | grep -o '"username":"[^"]*"' | cut -d ':' -f2- | tr -d '"' | tr -d ' ')
        TOKEN=$(echo "$line" | grep -o '"token":"[^"]*"' | cut -d ':' -f2- | tr -d '"' | tr -d ' ')
        if [[ "${#USERNAME}" -gt 0 && "${#TOKEN}" -gt 0 ]];then
                USERNAME=$(echo "$line" | grep -o '"username":"[^"]*"' | cut -d ':' -f2- | tr -d '"' | tr -d ' ')
        	TOKEN=$(echo "$line" | grep -o '"token":"[^"]*"' | cut -d ':' -f2- | tr -d '"' | tr -d ' ')
        	echo "$USERNAME:$TOKEN," >> ./usernameToToken.txt
	fi
done < ./logs/register.txt
