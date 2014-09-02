///HEX to DEC: Real

var num1, length, total, va1, i, j;

num1=string_upper(argument0)
while(string_copy(num1, 1, 1)="0"){num1=string_delete(num1, 1, 1)}

length=string_length(num1)
total=0

for(i=0; i!=length; i+=1)
{
    va1=1
    stringeses=string_copy(num1, 1, 1)
    num1=string_delete(num1, 1, 1)
    
    if stringeses="A"{stringeses="10"}
    if stringeses="B"{stringeses="11"}
    if stringeses="C"{stringeses="12"}
    if stringeses="D"{stringeses="13"}
    if stringeses="E"{stringeses="14"}
    if stringeses="F"{stringeses="15"}
    
    if length-i-2>=0{for(j=0; j!=length-i-2; j+=1){va1=va1*16}}
    
    if i+1!=length{total+=16*va1*real(stringeses)}
    else{total+=real(stringeses)}
}

return total

//ARG 0: HEX
