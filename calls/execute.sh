while IFS= read -r line; do
	echo "$line"
	eval "$line"
done < ../mockdata/mockposts.txt
