while IFS= read -r line; do
	./calls/register.sh 5191 "$line"
done < internal.txt
