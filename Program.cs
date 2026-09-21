using ChronicTruant.Simulation;
using ChronicTruant.History;
using ChronicTruant.StudentStrategy;
using ChronicTruant.Teachers;

Random random = new();

var factory = new TeacherFactory(random);

var teachers = Enum.GetValues<Subject>()
    .Select(subject => factory.Create(subject))
    .ToList();

ISkipStrategy strategy = new AlwaysAttendStrategy();

StudentHistory history = new StudentHistory();

DaySimulator daySimulator = new DaySimulator(teachers, strategy, history);

ConsoleWriter consoleWriter = new ConsoleWriter();

SemesterSimulator semesterSimulator = new SemesterSimulator(daySimulator, consoleWriter);

semesterSimulator.Run();


