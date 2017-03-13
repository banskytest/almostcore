#include <stdio.h>
#include <string.h>

int main()
{
   char src[40];
   char dest[100];
   char dest2[100];

   gets(src); 

   strcpy(dest, src); 
 
   strncpy(dest,src, strlen(dest)); //DevSkim: ignore DS111237,DS154189 
   
   strcat(dest, "this is bad");

   strncat(dest, "this is also bad", strlen(dest)); 

   memcpy(dest2, dest, sizeof dest);

   malloc(5);
   

   return(0);
}