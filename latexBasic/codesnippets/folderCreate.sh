mkdir tempfolder{1..100}
for ((x = 1; x <= 100; x++));
  do touch TempFolder$x/Testfile$x.txt;
done



int main(int argc, char const *argv[]) {
  if (argc > 2) {

  }
  return 0;
}
