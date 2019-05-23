using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PX.Data;
using PX.Data.BQL.Fluent;

namespace PX.Objects.HackathonDemo
{
    [PXHidden]
    public class Student : IBqlTable
    {

    }

    public class StudentsFBQLMaint : PXGraph<StudentsFBQLMaint, Student>
    {
		public SelectFrom<Student>.View Students;
    }
}
