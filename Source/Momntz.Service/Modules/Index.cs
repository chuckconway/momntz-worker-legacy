using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nancy;

namespace Momntz.Service.Modules
{
    public class Index : NancyModule
    {
        public Index()
        {
            //On startup check the queue for any items
            // add items to queue
            
            //#Processing item.
            //Change status of item in Queueu (maybe move to another queue)

            //resize images

            //save images to storage

            //insert image data into database

            //remove items from queue

            //delete queued image


            ThreadPool.QueueUserWorkItem(o =>
                {
                    Get["/"] = p => "Hi";
                });
        }
    }
}
